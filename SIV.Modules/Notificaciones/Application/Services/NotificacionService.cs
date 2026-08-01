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
    private readonly IUsuarioConsulta _usuarios;

    public NotificacionService(
        INotificacionRepository repo,
        ISeguimientoConsulta seguimientoConsulta,
        IUsuarioConsulta usuarios)
    {
        _repo = repo;
        _seguimientoConsulta = seguimientoConsulta;
        _usuarios = usuarios;
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

    public async Task<IEnumerable<RegistroNotificacionDto>> ObtenerRegistroAsync()
    {
        var todas = (await _repo.ObtenerTodasAsync()).ToList();

        // Se resuelve el correo de cada destinatario en una sola consulta (evita N+1).
        var ids = todas.Select(n => n.UsuarioId).Distinct().ToList();
        var correos = (await _usuarios.ObtenerContactosAsync(ids)).ToDictionary(c => c.Id, c => c.Email);

        return todas.Select(n => new RegistroNotificacionDto(
            n.Id,
            n.UsuarioId,
            correos.GetValueOrDefault(n.UsuarioId, "—"),
            n.VueloId,
            n.Mensaje,
            n.Estado.ToString(),
            n.GeneradaEn,
            n.LeidaEn));
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
