using Microsoft.AspNetCore.Mvc;
using SIV.Modules.Notificaciones.Application;

namespace SIV.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _servicio;

    public NotificacionesController(INotificacionService servicio)
    {
        _servicio = servicio;
    }

    // devuelve todas las notificaciones de un usuario
    [HttpGet("{usuarioId:guid}")]
    public async Task<IActionResult> ObtenerNotificaciones(Guid usuarioId)
    {
        var notificaciones = await _servicio.ObtenerNotificacionesAsync(usuarioId);
        return Ok(notificaciones);
    }

    // marca una notificacion como leida
    [HttpPatch("{id:guid}/leida")]
    public async Task<IActionResult> MarcarComoLeida(Guid id)
    {
        await _servicio.MarcarComoLeidaAsync(id);
        return NoContent();
    }
}
