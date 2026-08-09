using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SIV.API.Auth;
using SIV.API.Seguridad;
using SIV.Modules.Notificaciones.Application.Interfaces;
using SIV.Shared.Contracts;

namespace SIV.API.Controllers;

// RNF-SEG-01: consultar notificaciones propias requiere autenticación (CU-NOT-03).
// Excepción: los endpoints [AllowAnonymous] del final, para el visitante que sigue
// vuelos sin cuenta (ver SeguimientoController para el diseño del id de dispositivo).
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificacionesController : ControllerBase
{
    private readonly INotificacionService _servicio;
    private readonly IUsuarioConsulta _usuarios;

    public NotificacionesController(INotificacionService servicio, IUsuarioConsulta usuarios)
    {
        _servicio = servicio;
        _usuarios = usuarios;
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
    // RNF-SEG-03: solo se puede marcar la propia. Sin esta comprobación, cualquier
    // usuario con sesión podía marcar como leída la notificación de otro conociendo
    // su id, que es el mismo IDOR que ya se cerró en las consultas.
    [HttpPatch("{id:guid}/leida")]
    public async Task<IActionResult> MarcarComoLeida(Guid id)
    {
        if (!User.EsAdministrador() && !await EsSuyaAsync(id, User.ObtenerUsuarioId()))
            return Forbid();

        await _servicio.MarcarComoLeidaAsync(id);
        return NoContent();
    }

    // ─────────── Notificaciones del visitante sin cuenta ───────────
    // Mismo servicio que las de un usuario: para el módulo, el destinatario es un
    // id y punto. Lo único que cambia es cómo se comprueba quién pregunta.

    [AllowAnonymous]
    [HttpGet("anonimo/{dispositivoId:guid}")]
    public async Task<IActionResult> ObtenerPorDispositivo(Guid dispositivoId)
    {
        if (!await EsDispositivoAnonimoAsync(dispositivoId))
            return Forbid();

        return Ok(await _servicio.ObtenerNotificacionesAsync(dispositivoId));
    }

    [AllowAnonymous]
    [EnableRateLimiting(LimitesPeticiones.Anonimo)]
    [HttpPatch("anonimo/{id:guid}/leida")]
    public async Task<IActionResult> MarcarComoLeidaAnonima(Guid id, [FromBody] MarcarLeidaAnonimaRequest request)
    {
        if (!await EsDispositivoAnonimoAsync(request.DispositivoId))
            return Forbid();

        if (!await EsSuyaAsync(id, request.DispositivoId))
            return Forbid();

        await _servicio.MarcarComoLeidaAsync(id);
        return NoContent();
    }

    /// <summary>
    /// Un id solo vale como identificador anónimo si no pertenece a una cuenta
    /// registrada; si no, la vía anónima serviría para leer notificaciones ajenas.
    /// </summary>
    private async Task<bool> EsDispositivoAnonimoAsync(Guid id)
    {
        if (id == Guid.Empty) return false;
        var contactos = await _usuarios.ObtenerContactosAsync([id]);
        return contactos.Count == 0;
    }

    /// <summary>
    /// Comprueba que la notificación pertenezca a quien la está tocando. Vale
    /// igual para el usuario con cuenta y para el visitante anónimo, porque para
    /// el módulo de Notificaciones el destinatario es un id y punto.
    /// </summary>
    private async Task<bool> EsSuyaAsync(Guid notificacionId, Guid? propietarioId)
    {
        // Sin identidad no hay propiedad que comprobar: se niega.
        if (propietarioId is not { } id || id == Guid.Empty) return false;
        var propietario = id;

        var propias = await _servicio.ObtenerNotificacionesAsync(propietario);
        return propias.Any(n => n.Id == notificacionId);
    }
}

public record MarcarLeidaAnonimaRequest(Guid DispositivoId);
