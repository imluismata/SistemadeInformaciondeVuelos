using SIV.Modules.Notificaciones.Domain;
using SIV.Modules.Seguimiento.Application;
using SIV.Shared.Events;

namespace SIV.Modules.Notificaciones.Application;

public class NotificacionService : INotificacionService
{
    private readonly INotificacionRepository _repo;
    private readonly ISeguimientoService _seguimientoService;

    public NotificacionService(
        INotificacionRepository repo,
        ISeguimientoService seguimientoService)
    {
        _repo = repo;
        _seguimientoService = seguimientoService;
    }

    public async Task GenerarNotificacionesAsync(IVueloCambiadoEvento evento)
    {
        var usuariosInteresados = await _seguimientoService
            .ObtenerUsuariosPorVueloAsync(evento.VueloId);

        foreach (var usuarioId in usuariosInteresados)
        {
            var mensaje = ConstruirMensaje(evento);
            var notificacion = Notificacion.Crear(usuarioId, evento.VueloId, mensaje);
            await _repo.AgregarAsync(notificacion);
        }

        await _repo.GuardarCambiosAsync();
    }

    public async Task<IEnumerable<Notificacion>> ObtenerNotificacionesAsync(Guid usuarioId)
    {
        return await _repo.ObtenerPorUsuarioAsync(usuarioId);
    }

    public async Task MarcarComoLeidaAsync(Guid notificacionId)
    {
        var notificacion = await _repo.ObtenerPorIdAsync(notificacionId)
            ?? throw new InvalidOperationException(
                $"No existe una notificación con Id {notificacionId}.");

        notificacion.MarcarComoLeida();
        await _repo.ActualizarAsync(notificacion);
        await _repo.GuardarCambiosAsync();
    }

    private static string ConstruirMensaje(IVueloCambiadoEvento evento)
    {
        return evento.TipoCambio switch
        {
            "Cancelacion" => $"El vuelo fue cancelado. Motivo: {evento.Causa}",
            "Retraso" => $"El vuelo fue retrasado. Causa: {evento.Causa}",
            "Adelanto" => $"El vuelo fue adelantado. Causa: {evento.Causa}",
            "CambioDePuerta" => $"El vuelo cambió de puerta. {evento.Causa}",
            _ => $"Actualización en el vuelo: {evento.Causa}"
        };
    }
}