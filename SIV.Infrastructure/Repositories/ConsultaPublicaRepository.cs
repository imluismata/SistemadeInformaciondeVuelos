using Microsoft.EntityFrameworkCore;
using SIV.Infrastructure.Configuracion;
using SIV.Modules.Catalogo.Domain;
using SIV.Modules.ConsultaPublica.Application;
using SIV.Modules.ConsultaPublica.Application.Dtos;
using SIV.Modules.Vuelos.Domain;

namespace SIV.Infrastructure.Repositories;

internal class ConsultaPublicaRepository(SivDbContext context, OpcionesAeropuerto opcionesAeropuerto)
    : IConsultaPublicaRepository
{
    // Ventana móvil de 24 horas: entre 24 h en el pasado (para seguir visibles
    // hasta 24 h después de culminados) y 24 h en el futuro.
    private static (DateTime inferior, DateTime superior) VentanaVeinticuatroHoras()
    {
        var ahora = DateTime.Now;
        return (ahora.AddHours(-24), ahora.AddHours(24));
    }

    public async Task<IEnumerable<VueloPublicoDto>> ObtenerVuelosActivosAsync()
    {
        var (inferior, superior) = VentanaVeinticuatroHoras();

        var vuelos = await context.Vuelos
            .Where(v => v.HorarioLlegada >= inferior && v.HorarioSalida <= superior)
            .OrderBy(v => v.HorarioSalida)
            .ToListAsync();

        return await MapearAsync(vuelos);
    }

    public async Task<IEnumerable<VueloPublicoDto>> ObtenerSalidasAsync()
    {
        var aeropuertoBaseId = await ObtenerIdAeropuertoBaseAsync();
        if (aeropuertoBaseId is null) return [];

        var (inferior, superior) = VentanaVeinticuatroHoras();

        // Salidas = vuelos que despegan DESDE el aeropuerto base (AILA).
        var vuelos = await context.Vuelos
            .Where(v => v.AeropuertoOrigenId == aeropuertoBaseId
                     && v.HorarioLlegada >= inferior && v.HorarioSalida <= superior)
            .OrderBy(v => v.HorarioSalida)
            .ToListAsync();

        return await MapearAsync(vuelos);
    }

    public async Task<IEnumerable<VueloPublicoDto>> ObtenerLlegadasAsync()
    {
        var aeropuertoBaseId = await ObtenerIdAeropuertoBaseAsync();
        if (aeropuertoBaseId is null) return [];

        var (inferior, superior) = VentanaVeinticuatroHoras();

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

        var aerolineas = await context.Aerolineas
            .Where(a => aerolineaIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Nombre);

        var aeropuertos = await context.Aeropuertos
            .Where(a => aeropuertoIds.Contains(a.Id))
            .ToDictionaryAsync(a => a.Id, a => a.Nombre);

        return vuelos.Select(v => new VueloPublicoDto(
            v.Id,
            v.Numero,
            aerolineas.GetValueOrDefault(v.AerolineaId, "—"),
            aeropuertos.GetValueOrDefault(v.AeropuertoOrigenId, "—"),
            aeropuertos.GetValueOrDefault(v.AeropuertoDestinoId, "—"),
            v.HorarioSalida,
            v.HorarioLlegada,
            v.Puerta,
            v.EstadoActual.ToString()
        )).ToList();
    }
}
