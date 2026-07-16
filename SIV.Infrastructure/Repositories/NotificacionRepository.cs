using Microsoft.EntityFrameworkCore;
using SIV.Modules.Notificaciones.Application;
using SIV.Modules.Notificaciones.Domain;

namespace SIV.Infrastructure.Repositories;

internal class NotificacionRepository : INotificacionRepository
{
    private readonly SivDbContext _context;

    public NotificacionRepository(SivDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(Notificacion notificacion)
    {
        await _context.Notificaciones.AddAsync(notificacion);
    }

    public async Task<IEnumerable<Notificacion>> ObtenerPorUsuarioAsync(Guid usuarioId)
    {
        return await _context.Notificaciones
            .Where(n => n.UsuarioId == usuarioId)
            .OrderByDescending(n => n.GeneradaEn)
            .ToListAsync();
    }

    public async Task<Notificacion?> ObtenerPorIdAsync(Guid id)
    {
        return await _context.Notificaciones
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    public async Task ActualizarAsync(Notificacion notificacion)
    {
        _context.Notificaciones.Update(notificacion);
        await Task.CompletedTask;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
