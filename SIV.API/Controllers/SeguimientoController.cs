using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SIV.API.Auth;
using SIV.API.Seguridad;
using SIV.Modules.Seguimiento.Application.Dtos;
using SIV.Modules.Seguimiento.Application.Interfaces;
using SIV.Shared.Contracts;

namespace SIV.API.Controllers;

// RNF-SEG-01: seguir vuelos es una función operativa del Usuario Registrado; requiere autenticación.
// Las excepciones son los endpoints marcados [AllowAnonymous] del final, para el
// visitante sin cuenta.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SeguimientoController : ControllerBase
{
    private readonly ISeguimientoService _servicio;
    private readonly IUsuarioConsulta _usuarios;

    public SeguimientoController(ISeguimientoService servicio, IUsuarioConsulta usuarios)
    {
        _servicio = servicio;
        _usuarios = usuarios;
    }

    [HttpPost]
    public async Task<IActionResult> RegistrarSeguimiento(
        [FromBody] RegistrarSeguimientoRequest request)
    {
        // RNF-SEG-03: un usuario solo puede gestionar sus propios seguimientos.
        if (!EsPropioUsuario(request.UsuarioId))
            return Forbid();

        await _servicio.RegistrarAsync(new RegistrarSeguimientoDto(request.UsuarioId, request.VueloId));
        return Created(string.Empty, null);
    }

    [HttpDelete]
    public async Task<IActionResult> CancelarSeguimiento(
        [FromBody] CancelarSeguimientoRequest request)
    {
        if (!EsPropioUsuario(request.UsuarioId))
            return Forbid();

        await _servicio.CancelarAsync(new CancelarSeguimientoDto(request.UsuarioId, request.VueloId));
        return NoContent();
    }

    [HttpGet("usuario/{usuarioId:guid}")]
    public async Task<IActionResult> ObtenerPorUsuario(Guid usuarioId)
    {
        if (!EsPropioUsuario(usuarioId))
            return Forbid();

        var seguimientos = await _servicio.ObtenerPorUsuarioAsync(usuarioId);
        return Ok(seguimientos);
    }

    // CU-SEG-04: la consulta de seguidores por vuelo es de análisis/auditoría.
    [Authorize(Roles = "Administrador,Auditor")]
    [HttpGet("vuelo/{vueloId:guid}")]
    public async Task<IActionResult> ObtenerUsuariosPorVuelo(Guid vueloId)
    {
        var usuarios = await _servicio.ObtenerUsuariosPorVueloAsync(vueloId);
        return Ok(usuarios);
    }

    // CU-SEG-04: historial completo de seguimientos (usuario, vuelo, estado) para
    // análisis y auditoría. Reservado a admin/auditor.
    [Authorize(Roles = "Administrador,Auditor")]
    [HttpGet("todos")]
    public async Task<IActionResult> ObtenerTodos()
        => Ok(await _servicio.ObtenerTodosAsync());

    // ─────────────── Seguimiento sin cuenta (visitante anónimo) ───────────────
    //
    // El portal genera un Guid aleatorio por navegador ("id de dispositivo") y lo
    // usa como identificador del seguidor. El dominio no necesitó cambios: un
    // seguimiento siempre fue la pareja (seguidor, vuelo), y ese seguidor puede
    // ser una cuenta o un dispositivo. Por eso estos endpoints llaman exactamente
    // al mismo servicio que los autenticados.
    //
    // El correo tampoco necesitó un caso especial: el envío resuelve destinatarios
    // contra la tabla de usuarios, y un id de dispositivo no está ahí, así que al
    // visitante anónimo solo le llega la notificación in-app. Es lo que se quería.
    //
    // Riesgo a cubrir: como el id lo propone el cliente, alguien podría mandar el
    // Guid de un usuario registrado y leer sus seguimientos por la puerta anónima.
    // Por eso todo endpoint anónimo rechaza los ids que pertenecen a una cuenta.

    [AllowAnonymous]
    [EnableRateLimiting(LimitesPeticiones.Anonimo)]
    [HttpPost("anonimo")]
    public async Task<IActionResult> RegistrarSeguimientoAnonimo(
        [FromBody] SeguimientoAnonimoRequest request)
    {
        if (!await EsDispositivoAnonimoAsync(request.DispositivoId))
            return Forbid();

        await _servicio.RegistrarAsync(new RegistrarSeguimientoDto(request.DispositivoId, request.VueloId));
        return Created(string.Empty, null);
    }

    [AllowAnonymous]
    [EnableRateLimiting(LimitesPeticiones.Anonimo)]
    [HttpDelete("anonimo")]
    public async Task<IActionResult> CancelarSeguimientoAnonimo(
        [FromBody] SeguimientoAnonimoRequest request)
    {
        if (!await EsDispositivoAnonimoAsync(request.DispositivoId))
            return Forbid();

        await _servicio.CancelarAsync(new CancelarSeguimientoDto(request.DispositivoId, request.VueloId));
        return NoContent();
    }

    [AllowAnonymous]
    [HttpGet("anonimo/{dispositivoId:guid}")]
    public async Task<IActionResult> ObtenerPorDispositivo(Guid dispositivoId)
    {
        if (!await EsDispositivoAnonimoAsync(dispositivoId))
            return Forbid();

        return Ok(await _servicio.ObtenerPorUsuarioAsync(dispositivoId));
    }

    // El usuario solo puede operar sobre su propia cuenta; el administrador puede sobre cualquiera.
    private bool EsPropioUsuario(Guid usuarioId)
        => User.EsAdministrador() || User.ObtenerUsuarioId() == usuarioId;

    /// <summary>
    /// Un id sirve como identificador anónimo solo si NO corresponde a una cuenta
    /// registrada. Así la vía anónima nunca puede usarse para espiar a un usuario.
    /// </summary>
    private async Task<bool> EsDispositivoAnonimoAsync(Guid id)
    {
        if (id == Guid.Empty) return false;
        var contactos = await _usuarios.ObtenerContactosAsync([id]);
        return contactos.Count == 0;
    }
}

public record RegistrarSeguimientoRequest(Guid UsuarioId, Guid VueloId);
public record CancelarSeguimientoRequest(Guid UsuarioId, Guid VueloId);

public record SeguimientoAnonimoRequest(
    [Required(ErrorMessage = "El identificador del dispositivo es obligatorio.")]
    Guid DispositivoId,

    [Required(ErrorMessage = "El vuelo es obligatorio.")]
    Guid VueloId);
