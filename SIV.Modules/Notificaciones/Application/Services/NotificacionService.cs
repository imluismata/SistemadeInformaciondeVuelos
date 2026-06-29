using SIV.Modules.Notificaciones.Application.Base;
using SIV.Modules.Notificaciones.Application.Dtos;
using SIV.Modules.Notificaciones.Application.Interfaces;
using SIV.Modules.Notificaciones.Domain;
using SIV.Shared.Contracts;
using SIV.Shared.Events;

namespace SIV.Modules.Notificaciones.Application.Services;

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
            var notificacion = Notificacion.Crear(usuarioId, evento.VueloId, MensajeNotificacion.Construir(evento));
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

    private static NotificacionDto MapToDto(Notificacion n) =>
        new(n.Id, n.VueloId, n.Mensaje, n.Estado.ToString(), n.GeneradaEn, n.LeidaEn);
}
