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
        if (db.Entry(aerolinea).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
        {
            var existe = await db.Aerolineas.AnyAsync(a => a.Id == aerolinea.Id);
            if (!existe)
                db.Aerolineas.Add(aerolinea);
            else
                db.Aerolineas.Update(aerolinea);
        }

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
        if (db.Entry(aeropuerto).State == Microsoft.EntityFrameworkCore.EntityState.Detached)
        {
            var existe = await db.Aeropuertos.AnyAsync(a => a.Id == aeropuerto.Id);
            if (!existe)
                db.Aeropuertos.Add(aeropuerto);
            else
                db.Aeropuertos.Update(aeropuerto);
        }

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

    // ---------- Terminales ----------
    public async Task<IReadOnlyList<Terminal>> ObtenerTerminalesAsync()
        => await db.Terminales.ToListAsync();

    public async Task<Terminal?> ObtenerTerminalPorIdAsync(Guid id)
        => await db.Terminales.FindAsync(id);

    public async Task<Terminal?> ObtenerTerminalPorCodigoAsync(string codigo)
        => await db.Terminales.FirstOrDefaultAsync(t => t.Codigo == codigo);

    public async Task GuardarTerminalAsync(Terminal terminal)
    {
        if (db.Entry(terminal).State == EntityState.Detached)
        {
            var existe = await db.Terminales.AnyAsync(t => t.Id == terminal.Id);
            if (!existe) db.Terminales.Add(terminal); else db.Terminales.Update(terminal);
        }
        await db.SaveChangesAsync();
    }

    // ---------- Puertas ----------
    public async Task<IReadOnlyList<Puerta>> ObtenerPuertasAsync()
        => await db.Puertas.ToListAsync();

    public async Task<Puerta?> ObtenerPuertaPorIdAsync(Guid id)
        => await db.Puertas.FindAsync(id);

    public async Task<Puerta?> ObtenerPuertaPorCodigoAsync(string codigo)
        => await db.Puertas.FirstOrDefaultAsync(p => p.Codigo == codigo);

    public async Task GuardarPuertaAsync(Puerta puerta)
    {
        if (db.Entry(puerta).State == EntityState.Detached)
        {
            var existe = await db.Puertas.AnyAsync(p => p.Id == puerta.Id);
            if (!existe) db.Puertas.Add(puerta); else db.Puertas.Update(puerta);
        }
        await db.SaveChangesAsync();
    }
}
