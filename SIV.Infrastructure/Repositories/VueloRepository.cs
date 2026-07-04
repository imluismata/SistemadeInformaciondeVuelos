using Microsoft.EntityFrameworkCore;
using SIV.Modules.Vuelos.Application;
using SIV.Modules.Vuelos.Domain;

namespace SIV.Infrastructure.Repositories;

internal sealed class VueloRepository(SivDbContext db) : IVueloRepository
{
    public async Task<IReadOnlyList<Vuelo>> ObtenerTodosAsync()
        => await db.Vuelos.ToListAsync();

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
        => await db.Vuelos.FirstOrDefaultAsync(v => v.Id == id);

    public async Task<Vuelo?> ObtenerPorNumeroAsync(string numero)
        => await db.Vuelos
            .FirstOrDefaultAsync(v => v.Numero == numero);

    public async Task GuardarAsync(Vuelo vuelo)
    {
        var existe = await db.Vuelos.AnyAsync(v => v.Id == vuelo.Id);
        if (!existe)
            db.Vuelos.Add(vuelo);
        else
            db.Vuelos.Update(vuelo);

        await db.SaveChangesAsync();
    }
}
