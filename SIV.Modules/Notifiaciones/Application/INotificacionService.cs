using SIV.Modules.Notificaciones.Domain;
using SIV.Shared.Events;

namespace SIV.Modules.Notificaciones.Application;

public interface INotificacionService
{
    Task GenerarNotificacionesAsync(IVueloCambiadoEvento evento);
    Task<IEnumerable<Notificacion>> ObtenerNotificacionesAsync(Guid usuarioId);
    Task MarcarComoLeidaAsync(Guid notificacionId);
}