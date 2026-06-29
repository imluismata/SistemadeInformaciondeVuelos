using Microsoft.EntityFrameworkCore;
using SIV.Modules.Auditoria.Application;
using SIV.Modules.Auditoria.Domain;

namespace SIV.Infrastructure.Repositories;

public sealed class AuditoriaRepository(SivDbContext db) : IAuditoriaRepository
{
    public async Task GuardarAsync(RegistroAuditoria registro)
    {
        db.Auditoria.Add(registro);
        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<RegistroAuditoria>> ConsultarAsync(string? modulo, string? accion, DateTime? desde, DateTime? hasta)
    {
        var query = db.Auditoria.AsQueryable();

        if (!string.IsNullOrWhiteSpace(modulo))
            query = query.Where(r => r.Modulo == modulo);

        if (!string.IsNullOrWhiteSpace(accion))
            query = query.Where(r => r.Accion == accion);

        if (desde.HasValue)
            query = query.Where(r => r.FechaHora >= desde.Value);

        if (hasta.HasValue)
            query = query.Where(r => r.FechaHora <= hasta.Value);

        return await query.OrderByDescending(r => r.FechaHora).ToListAsync();
    }
}
