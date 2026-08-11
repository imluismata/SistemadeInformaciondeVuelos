using System.Globalization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using SIV.Infrastructure.Configuracion;
using SIV.Modules.Catalogo.Domain;
using SIV.Modules.ConsultaPublica.Application;
using SIV.Modules.ConsultaPublica.Application.Dtos;
using SIV.Modules.Vuelos.Domain;
using SIV.Shared.Enums;

namespace SIV.Infrastructure.Repositories;

internal class ConsultaPublicaRepository(SivDbContext context, OpcionesAeropuerto opcionesAeropuerto)
    : IConsultaPublicaRepository
{
    // Sin fecha: ventana móvil de 24 horas (para el tablero en vivo, entre 24h
    // en el pasado y 24h en el futuro). Con fecha: ese día completo, de 00:00 a
    // 23:59:59 — quien pide explícitamente "el sábado" no quiere que el
    // resultado dependa de qué hora es "ahora"; antes esta ventana se aplicaba
    // siempre, así que buscar un día fuera de las 24h vigentes devolvía vacío
    // aunque hubiera vuelos ese día.
    private static (DateTime inferior, DateTime superior) Ventana(DateTime? fecha)
    {
        if (fecha.HasValue)
        {
            var dia = fecha.Value.Date;
            return (dia, dia.AddDays(1).AddTicks(-1));
        }

        var ahora = DateTime.Now;
        return (ahora.AddHours(-24), ahora.AddHours(24));
    }

    public async Task<IEnumerable<VueloPublicoDto>> ObtenerVuelosActivosAsync(DateTime? fecha = null)
    {
        var (inferior, superior) = Ventana(fecha);

        var vuelos = await context.Vuelos
            .Where(v => v.HorarioLlegada >= inferior && v.HorarioSalida <= superior)
            .OrderBy(v => v.HorarioSalida)
            .ToListAsync();

        return await MapearAsync(vuelos);
    }

    public async Task<IEnumerable<VueloPublicoDto>> ObtenerSalidasAsync(DateTime? fecha = null)
    {
        var aeropuertoBaseId = await ObtenerIdAeropuertoBaseAsync();
        if (aeropuertoBaseId is null) return [];

        var (inferior, superior) = Ventana(fecha);

        // Salidas = vuelos que despegan DESDE el aeropuerto base (AILA).
        var vuelos = await context.Vuelos
            .Where(v => v.AeropuertoOrigenId == aeropuertoBaseId
                     && v.HorarioLlegada >= inferior && v.HorarioSalida <= superior)
            .OrderBy(v => v.HorarioSalida)
            .ToListAsync();

        return await MapearAsync(vuelos);
    }

    public async Task<IEnumerable<VueloPublicoDto>> ObtenerLlegadasAsync(DateTime? fecha = null)
    {
        var aeropuertoBaseId = await ObtenerIdAeropuertoBaseAsync();
        if (aeropuertoBaseId is null) return [];

        var (inferior, superior) = Ventana(fecha);

        // Llegadas = vuelos que aterrizan EN el aeropuerto base (AILA).
        var vuelos = await context.Vuelos
            .Where(v => v.AeropuertoDestinoId == aeropuertoBaseId
                     && v.HorarioLlegada >= inferior && v.HorarioSalida <= superior)
            .OrderBy(v => v.HorarioLlegada)
            .ToListAsync();

        return await MapearAsync(vuelos);
    }

    private async Task<Guid?> ObtenerIdAeropuertoBaseAsync()
    {
        var codigo = opcionesAeropuerto.Codigo;
        var aeropuerto = await context.Aeropuertos
            .FirstOrDefaultAsync(a => a.Codigo == codigo);
        return aeropuerto?.Id;
    }

    public async Task<VueloPublicoDto?> BuscarPorNumeroAsync(string numeroVuelo)
    {
        var vuelo = await context.Vuelos
            .FirstOrDefaultAsync(v => v.Numero == numeroVuelo);

        if (vuelo is null) return null;

        var lista = await MapearAsync([vuelo]);
        return lista.FirstOrDefault();
    }

    public async Task<VueloPublicoDto?> ObtenerPorIdAsync(Guid vueloId)
    {
        var vuelo = await context.Vuelos
            .FirstOrDefaultAsync(v => v.Id == vueloId);

        if (vuelo is null) return null;

        var lista = await MapearAsync([vuelo]);
        return lista.FirstOrDefault();
    }

    private async Task<List<VueloPublicoDto>> MapearAsync(IEnumerable<Vuelo> vuelos)
    {
        var aerolineaIds = vuelos.Select(v => v.AerolineaId).Distinct().ToList();
        var aeropuertoIds = vuelos.SelectMany(v => new[] { v.AeropuertoOrigenId, v.AeropuertoDestinoId }).Distinct().ToList();

        // Se traen nombre y código juntos: las pantallas usan el código (MIA, B6)
        // y las páginas del portal el nombre completo.
        var aerolineas = await context.Aerolineas
            .Where(a => aerolineaIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => new { a.Nombre, a.Codigo });

        var aeropuertos = await context.Aeropuertos
            .Where(a => aeropuertoIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => new { a.Nombre, a.Codigo });

        return vuelos.Select(v =>
        {
            var aerolinea = aerolineas.GetValueOrDefault(v.AerolineaId);
            var origen = aeropuertos.GetValueOrDefault(v.AeropuertoOrigenId);
            var destino = aeropuertos.GetValueOrDefault(v.AeropuertoDestinoId);
            var (salidaOriginal, llegadaOriginal) = HorariosOriginales(v);

            return new VueloPublicoDto(
                v.Id,
                v.Numero,
                aerolinea?.Nombre ?? "—",
                aerolinea?.Codigo ?? "",
                origen?.Nombre ?? "—",
                origen?.Codigo ?? "",
                destino?.Nombre ?? "—",
                destino?.Codigo ?? "",
                v.HorarioSalida,
                v.HorarioLlegada,
                salidaOriginal,
                llegadaOriginal,
                v.PuertaDescripcion,
                v.EstadoActual.ToString());
        }).ToList();
    }

    /// <summary>
    /// Recupera las horas que tenía el vuelo antes de su primer retraso o
    /// adelanto, para poder mostrar en pantalla la programada junto a la nueva.
    ///
    /// El dominio no guarda la hora original: al registrar un retraso, mueve
    /// HorarioSalida y HorarioLlegada hacia adelante. Lo único que queda del
    /// valor anterior es el historial de cambios operativos, así que se lee de
    /// ahí. Solo cuentan Retraso y Adelanto: son las desviaciones sobre el
    /// itinerario publicado, mientras que una edición de datos es una corrección
    /// del itinerario en sí.
    ///
    /// Es una solución de lectura, sin migración ni cambios en el módulo de
    /// Vuelos, pero depende del formato de texto con que ese módulo escribe el
    /// valor anterior. Si nunca se pudo interpretar, se devuelve null y la
    /// pantalla simplemente muestra una sola hora — nunca un dato inventado.
    /// </summary>
    private static (DateTime? Salida, DateTime? Llegada) HorariosOriginales(Vuelo vuelo)
    {
        var primeraDesviacion = vuelo.CambiosOperativos
            .Where(c => c.Tipo is TipoCambioOperativo.Retraso or TipoCambioOperativo.Adelanto)
            .OrderBy(c => c.RegistradoEn)
            .FirstOrDefault();

        if (primeraDesviacion?.ValorAnterior is null)
            return (null, null);

        return (
            LeerHora(primeraDesviacion.ValorAnterior, "Salida"),
            LeerHora(primeraDesviacion.ValorAnterior, "Llegada"));
    }

    /// <summary>Extrae "Campo=&lt;fecha ISO&gt;" de la cadena del historial.</summary>
    private static DateTime? LeerHora(string texto, string campo)
    {
        var coincidencia = Regex.Match(texto, $@"{campo}=([^;]+)");
        if (!coincidencia.Success)
            return null;

        return DateTime.TryParse(
            coincidencia.Groups[1].Value.Trim(),
            CultureInfo.InvariantCulture,
            DateTimeStyles.RoundtripKind,
            out var hora)
            ? hora
            : null;
    }
}
