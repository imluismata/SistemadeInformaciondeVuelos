using SIV.Modules.Notificaciones.Domain;

namespace SIV.Modules.Notificaciones.Application;

public interface INotificacionRepository
{
    Task AgregarAsync(Notificacion notificacion);
    Task<IEnumerable<Notificacion>> ObtenerPorUsuarioAsync(Guid usuarioId);
    // Registro completo del sistema (CU-NOT-04), para admin/auditor.
    Task<IEnumerable<Notificacion>> ObtenerTodasAsync();
    Task<Notificacion?> ObtenerPorIdAsync(Guid id);
    Task ActualizarAsync(Notificacion notificacion);
    Task GuardarCambiosAsync();
}