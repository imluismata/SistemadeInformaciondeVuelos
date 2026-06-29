using SIV.Shared.Events;

namespace SIV.Modules.Notificaciones.Application;

public interface INotificacionService
{
    Task GenerarNotificacionesAsync(IVueloCambiadoEvento evento);
    Task<IEnumerable<NotificacionDto>> ObtenerNotificacionesAsync(Guid usuarioId);
    Task MarcarComoLeidaAsync(Guid notificacionId);
}
