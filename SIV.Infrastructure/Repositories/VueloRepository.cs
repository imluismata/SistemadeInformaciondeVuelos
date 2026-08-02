using Microsoft.EntityFrameworkCore;
using SIV.Modules.Vuelos.Application;
using SIV.Modules.Vuelos.Domain;
using SIV.Shared.Enums;

namespace SIV.Infrastructure.Repositories;

internal sealed class VueloRepository(SivDbContext db) : IVueloRepository
{
    public async Task<IReadOnlyList<Vuelo>> ObtenerTodosAsync()
        => await db.Vuelos.ToListAsync();

    public async Task<(IReadOnlyList<Vuelo> Items, int Total)> ObtenerPaginadoAsync(int pagina, int tamano)
    {
        // Ordeno por horario para que las paginas salgan siempre igual.
        var query = db.Vuelos.OrderBy(v => v.HorarioSalida);
        var total = await query.CountAsync();      // cuantos vuelos hay en total
        var items = await query
            .Skip((pagina - 1) * tamano)   // me salto las paginas anteriores
            .Take(tamano)                  // agarro solo los de esta pagina
            .ToListAsync();
        return (items, total);
    }

    public async Task<IReadOnlyList<Vuelo>> ConsultarAsync(ConsultarVuelosQuery filtro)
    {
        var query = db.Vuelos.AsQueryable();

        if (filtro.AerolineaId.HasValue)
            query = query.Where(v => v.AerolineaId == filtro.AerolineaId.Value);

        if (filtro.AeropuertoOrigenId.HasValue)
            query = query.Where(v => v.AeropuertoOrigenId == filtro.AeropuertoOrigenId.Value);

        if (filtro.AeropuertoDestinoId.HasValue)
            query = query.Where(v => v.AeropuertoDestinoId == filtro.AeropuertoDestinoId.Value);

        if (filtro.FechaDesde.HasValue)
            query = query.Where(v => v.HorarioSalida >= filtro.FechaDesde.Value);

        if (filtro.FechaHasta.HasValue)
            query = query.Where(v => v.HorarioSalida <= filtro.FechaHasta.Value);

        if (filtro.Estado.HasValue)
            query = query.Where(v => v.EstadoActual == filtro.Estado.Value);

        return await query.OrderBy(v => v.HorarioSalida).ToListAsync();
    }

    public async Task<Vuelo?> ObtenerPorIdAsync(Guid id)
        => await db.Vuelos
            .Include(v => v.HistorialEstados)
            .Include(v => v.CambiosOperativos)
            .FirstOrDefaultAsync(v => v.Id == id);

    public async Task<bool> ExisteNumeroParaAerolineaYFechaAsync(string numero, Guid aerolineaId, DateTime fecha)
    {
        // Mismo número + misma aerolínea + mismo día de salida (comparación por rango
        // para que EF la traduzca a SQL sin depender de funciones de fecha).
        var dia = fecha.Date;
        var siguiente = dia.AddDays(1);
        return await db.Vuelos.AnyAsync(v =>
            v.Numero == numero
            && v.AerolineaId == aerolineaId
            && v.HorarioSalida >= dia && v.HorarioSalida < siguiente);
    }

    // Un vuelo cuenta como "activo" mientras no haya llegado a un estado final.
    private static readonly EstadoVuelo[] EstadosFinales = [EstadoVuelo.Completado, EstadoVuelo.Cancelado];

    public async Task<bool> ExistenVuelosActivosPorAerolineaAsync(Guid aerolineaId)
        => await db.Vuelos.AnyAsync(v =>
            v.AerolineaId == aerolineaId && !EstadosFinales.Contains(v.EstadoActual));

    public async Task<bool> ExistenVuelosActivosPorAeropuertoAsync(Guid aeropuertoId)
        => await db.Vuelos.AnyAsync(v =>
            (v.AeropuertoOrigenId == aeropuertoId || v.AeropuertoDestinoId == aeropuertoId)
            && !EstadosFinales.Contains(v.EstadoActual));

    public async Task GuardarAsync(Vuelo vuelo)
    {
        var existe = await db.Vuelos.AnyAsync(v => v.Id == vuelo.Id);
        if (!existe)
        {
            db.Vuelos.Add(vuelo);
        }
        else
        {
            db.Vuelos.Update(vuelo);
            foreach (var entry in db.ChangeTracker.Entries()
                .Where(e => e.Metadata.IsOwned() && e.State == Microsoft.EntityFrameworkCore.EntityState.Modified))
            {
                entry.State = Microsoft.EntityFrameworkCore.EntityState.Added;
            }
        }

        await db.SaveChangesAsync();
    }
}
