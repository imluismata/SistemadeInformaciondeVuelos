using SIV.Modules.Seguimiento.Domain;

namespace SIV.Modules.Seguimiento.Application;

public interface ISeguimientoRepository
{
    Task AgregarAsync(Domain.Seguimiento seguimiento);
    Task<Domain.Seguimiento?> ObtenerPorUsuarioYVueloAsync(Guid usuarioId, Guid vueloId);
    Task<IEnumerable<Domain.Seguimiento>> ObtenerActivosPorVueloAsync(Guid vueloId);
    Task<IEnumerable<Domain.Seguimiento>> ObtenerActivosPorUsuarioAsync(Guid usuarioId);
    // Historial completo (CU-SEG-04), para admin/auditor.
    Task<IEnumerable<Domain.Seguimiento>> ObtenerTodosAsync();
    Task ActualizarAsync(Domain.Seguimiento seguimiento);
    Task GuardarCambiosAsync();
}