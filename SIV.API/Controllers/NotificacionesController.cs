using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.API.Auth;
using SIV.Modules.Notificaciones.Application.Interfaces;

namespace SIV.API.Controllers;

// RNF-SEG-01: consultar notificaciones propias requiere autenticación (CU-NOT-03).
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _servicio;

    public NotificacionesController(INotificacionService servicio)
    {
        _servicio = servicio;
    }

    // CU-NOT-04 / RNF-TRZ-03: registro global de notificaciones (qué, a quién, cuándo).
    // Reservado a admin/auditor. La ruta "registro" no choca con {usuarioId:guid}.
    [Authorize(Roles = "Administrador,Auditor")]
    [HttpGet("registro")]
    public async Task<IActionResult> ObtenerRegistro()
        => Ok(await _servicio.ObtenerRegistroAsync());

    // devuelve todas las notificaciones de un usuario
    [HttpGet("{usuarioId:guid}")]
    public async Task<IActionResult> ObtenerNotificaciones(Guid usuarioId)
    {
        // RNF-SEG-03: solo el propio usuario (o un administrador) ve sus notificaciones.
        if (!User.EsAdministrador() && User.ObtenerUsuarioId() != usuarioId)
            return Forbid();

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
