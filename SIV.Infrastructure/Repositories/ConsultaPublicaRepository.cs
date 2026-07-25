using Microsoft.EntityFrameworkCore;
using SIV.Modules.Catalogo.Domain;
using SIV.Modules.ConsultaPublica.Application;
using SIV.Modules.ConsultaPublica.Application.Dtos;
using SIV.Modules.Vuelos.Domain;

namespace SIV.Infrastructure.Repositories;

internal class ConsultaPublicaRepository(SivDbContext context) : IConsultaPublicaRepository
{
    public async Task<IEnumerable<VueloPublicoDto>> ObtenerVuelosActivosAsync()
    {
        // Ventana móvil de 24 horas: se muestran los vuelos cuyo horario cae
        // entre 24 h en el pasado (para seguir visibles hasta 24 h después de
        // culminados) y 24 h en el futuro. Es el comportamiento de un tablero
        // de aeropuerto en tiempo real.
        var ahora = DateTime.Now;
        var limiteInferior = ahora.AddHours(-24);
        var limiteSuperior = ahora.AddHours(24);

        var vuelos = await context.Vuelos
            .Where(v => v.HorarioLlegada >= limiteInferior && v.HorarioSalida <= limiteSuperior)
            .OrderBy(v => v.HorarioSalida)
            .ToListAsync();

        return await MapearAsync(vuelos);
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
