using Microsoft.EntityFrameworkCore;
using SIV.Modules.Seguimiento.Application;
using SeguimientoEntidad = SIV.Modules.Seguimiento.Domain.Seguimiento;
using SIV.Modules.Seguimiento.Domain;

namespace SIV.Infrastructure.Repositories;

internal class SeguimientoRepository : ISeguimientoRepository
{
    private readonly SivDbContext _context;

    public SeguimientoRepository(SivDbContext context)
    {
        _context = context;
    }

    public async Task AgregarAsync(SeguimientoEntidad seguimiento)
    {
        await _context.Seguimientos.AddAsync(seguimiento);
    }

    public async Task<SeguimientoEntidad?> ObtenerPorUsuarioYVueloAsync(Guid usuarioId, Guid vueloId)
    {
        return await _context.Seguimientos
            .FirstOrDefaultAsync(s =>
                s.UsuarioId == usuarioId &&
                s.VueloId == vueloId &&
                s.Estado == EstadoSeguimiento.Activo);
    }

    public async Task<IEnumerable<SeguimientoEntidad>> ObtenerActivosPorVueloAsync(Guid vueloId)
    {
        return await _context.Seguimientos
            .Where(s => s.VueloId == vueloId && s.Estado == EstadoSeguimiento.Activo)
            .ToListAsync();
    }

    public async Task<IEnumerable<SeguimientoEntidad>> ObtenerActivosPorUsuarioAsync(Guid usuarioId)
    {
        return await _context.Seguimientos
            .Where(s => s.UsuarioId == usuarioId && s.Estado == EstadoSeguimiento.Activo)
            .ToListAsync();
    }

    public async Task ActualizarAsync(SeguimientoEntidad seguimiento)
    {
        _context.Seguimientos.Update(seguimiento);
        await Task.CompletedTask;
    }

    public async Task GuardarCambiosAsync()
    {
        await _context.SaveChangesAsync();
    }
}
