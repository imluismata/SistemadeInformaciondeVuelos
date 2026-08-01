using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.API.Auth;
using SIV.Modules.Seguimiento.Application.Dtos;
using SIV.Modules.Seguimiento.Application.Interfaces;

namespace SIV.API.Controllers;

// RNF-SEG-01: seguir vuelos es una función operativa del Usuario Registrado; requiere autenticación.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class SeguimientoController : ControllerBase
{
    private readonly ISeguimientoService _servicio;

    public SeguimientoController(ISeguimientoService servicio)
    {
        _servicio = servicio;
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

    // El usuario solo puede operar sobre su propia cuenta; el administrador puede sobre cualquiera.
    private bool EsPropioUsuario(Guid usuarioId)
        => User.EsAdministrador() || User.ObtenerUsuarioId() == usuarioId;
}

public record RegistrarSeguimientoRequest(Guid UsuarioId, Guid VueloId);
public record CancelarSeguimientoRequest(Guid UsuarioId, Guid VueloId);
