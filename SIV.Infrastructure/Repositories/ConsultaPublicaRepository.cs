using Microsoft.EntityFrameworkCore;
using SIV.Modules.Catalogo.Domain;
using SIV.Modules.ConsultaPublica.Application;
using SIV.Modules.ConsultaPublica.Application.Dtos;
using SIV.Modules.Vuelos.Domain;

namespace SIV.Infrastructure.Repositories;

internal class ConsultaPublicaRepository(SivDbContext context) : IConsultaPublicaRepository
{
    private static readonly EstadoVuelo[] EstadosActivos =
    [
        EstadoVuelo.Programado,
        EstadoVuelo.Embarcando,
        EstadoVuelo.EnVuelo,
        EstadoVuelo.Retrasado,
    ];

    public async Task<IEnumerable<VueloPublicoDto>> ObtenerVuelosActivosAsync()
    {
        var vuelos = await context.Vuelos
            .Where(v => EstadosActivos.Contains(v.EstadoActual))
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
