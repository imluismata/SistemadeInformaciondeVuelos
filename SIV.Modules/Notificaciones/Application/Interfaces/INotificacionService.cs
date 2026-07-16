using SIV.Modules.Notificaciones.Application.Dtos;
using SIV.Shared.Events;

namespace SIV.Modules.Notificaciones.Application.Interfaces;

public interface INotificacionService
{
    Task GenerarNotificacionesAsync(IVueloCambiadoEvento evento);
    Task<IEnumerable<NotificacionDto>> ObtenerNotificacionesAsync(Guid usuarioId);
    Task MarcarComoLeidaAsync(Guid notificacionId);
}
