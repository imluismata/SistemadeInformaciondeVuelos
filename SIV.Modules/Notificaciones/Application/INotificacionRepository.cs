using SIV.Modules.Notificaciones.Domain;

namespace SIV.Modules.Notificaciones.Application;

public interface INotificacionRepository
{
    Task AgregarAsync(Notificacion notificacion);
    Task<IEnumerable<Notificacion>> ObtenerPorUsuarioAsync(Guid usuarioId);
    Task<Notificacion?> ObtenerPorIdAsync(Guid id);
    Task ActualizarAsync(Notificacion notificacion);
    Task GuardarCambiosAsync();
}