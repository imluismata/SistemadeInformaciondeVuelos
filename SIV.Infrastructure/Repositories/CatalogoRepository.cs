using Microsoft.EntityFrameworkCore;
using SIV.Modules.Catalogo.Application;
using SIV.Modules.Catalogo.Domain;

namespace SIV.Infrastructure.Repositories;

internal sealed class CatalogoRepository(SivDbContext db) : ICatalogoRepository
{
    public async Task<IReadOnlyList<Aerolinea>> ObtenerAerolineasAsync()
        => await db.Aerolineas.ToListAsync();

    public async Task<Aerolinea?> ObtenerAerolineaPorIdAsync(Guid id)
        => await db.Aerolineas.FindAsync(id);

    public async Task<Aerolinea?> ObtenerAerolineaPorCodigoAsync(string codigo)
        => await db.Aerolineas.FirstOrDefaultAsync(a => a.Codigo == codigo);

    public async Task GuardarAerolineaAsync(Aerolinea aerolinea)
    {
        var existe = await db.Aerolineas.AnyAsync(a => a.Id == aerolinea.Id);
        if (!existe)
            db.Aerolineas.Add(aerolinea);
        else
            db.Aerolineas.Update(aerolinea);

        await db.SaveChangesAsync();
    }

    public async Task EliminarAerolineaAsync(Guid id)
    {
        var aerolinea = await db.Aerolineas.FindAsync(id);
        if (aerolinea is not null)
        {
            aerolinea.Desactivar();
            await db.SaveChangesAsync();
        }
    }

    public async Task<IReadOnlyList<Aeropuerto>> ObtenerAeropuertosAsync()
        => await db.Aeropuertos.ToListAsync();

    public async Task<Aeropuerto?> ObtenerAeropuertoPorIdAsync(Guid id)
        => await db.Aeropuertos.FindAsync(id);

    public async Task<Aeropuerto?> ObtenerAeropuertoPorCodigoAsync(string codigo)
        => await db.Aeropuertos.FirstOrDefaultAsync(a => a.Codigo == codigo);

    public async Task GuardarAeropuertoAsync(Aeropuerto aeropuerto)
    {
        var existe = await db.Aeropuertos.AnyAsync(a => a.Id == aeropuerto.Id);
        if (!existe)
            db.Aeropuertos.Add(aeropuerto);
        else
            db.Aeropuertos.Update(aeropuerto);

        await db.SaveChangesAsync();
    }

    public async Task EliminarAeropuertoAsync(Guid id)
    {
        var aeropuerto = await db.Aeropuertos.FindAsync(id);
        if (aeropuerto is not null)
        {
            aeropuerto.Desactivar();
            await db.SaveChangesAsync();
        }
    }
}
