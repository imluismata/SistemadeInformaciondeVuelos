using Microsoft.EntityFrameworkCore;
using SIV.Modules.Vuelos.Application;
using SIV.Modules.Vuelos.Domain;

namespace SIV.Infrastructure.Repositories;

public sealed class VueloRepository(SivDbContext db) : IVueloRepository
{
    public async Task<IReadOnlyList<Vuelo>> ObtenerTodosAsync()
        => await db.Vuelos.ToListAsync();

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
