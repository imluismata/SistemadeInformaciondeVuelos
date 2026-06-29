using SIV.Modules.Notificaciones.Domain;
using SIV.Shared.Contracts;
using SIV.Shared.Enums;
using SIV.Shared.Events;

namespace SIV.Modules.Notificaciones.Application;

internal class NotificacionService : INotificacionService
{
    private readonly INotificacionRepository _repo;
    private readonly ISeguimientoConsulta _seguimientoConsulta;

    public NotificacionService(
        INotificacionRepository repo,
        ISeguimientoConsulta seguimientoConsulta)
    {
        _repo = repo;
        _seguimientoConsulta = seguimientoConsulta;
    }

    public async Task GenerarNotificacionesAsync(IVueloCambiadoEvento evento)
    {
        var usuariosInteresados = await _seguimientoConsulta
            .ObtenerUsuariosPorVueloAsync(evento.VueloId);

        foreach (var usuarioId in usuariosInteresados)
        {
            var notificacion = Notificacion.Crear(usuarioId, evento.VueloId, ConstruirMensaje(evento));
            await _repo.AgregarAsync(notificacion);
        }

        await _repo.GuardarCambiosAsync();
    }

    public async Task<IEnumerable<NotificacionDto>> ObtenerNotificacionesAsync(Guid usuarioId)
    {
        var notificaciones = await _repo.ObtenerPorUsuarioAsync(usuarioId);
        return notificaciones.Select(MapToDto);
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

    private static string ConstruirMensaje(IVueloCambiadoEvento evento) =>
        evento.TipoCambio switch
        {
            TipoCambio.Cancelacion    => $"El vuelo fue cancelado. Motivo: {evento.Causa}",
            TipoCambio.Retraso        => $"El vuelo fue retrasado. Causa: {evento.Causa}",
            TipoCambio.Adelanto       => $"El vuelo fue adelantado. Causa: {evento.Causa}",
            TipoCambio.CambioDePuerta => $"El vuelo cambió de puerta. {evento.Causa}",
            _ => $"Actualización en el vuelo: {evento.Causa}"
        };

    private static NotificacionDto MapToDto(Notificacion n) =>
        new(n.Id, n.VueloId, n.Mensaje, n.Estado.ToString(), n.GeneradaEn, n.LeidaEn);
}
