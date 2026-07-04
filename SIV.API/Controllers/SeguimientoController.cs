using Microsoft.AspNetCore.Mvc;
using SIV.Modules.Seguimiento.Application.Dtos;
using SIV.Modules.Seguimiento.Application.Interfaces;

namespace SIV.API.Controllers;

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
        await _servicio.RegistrarAsync(new RegistrarSeguimientoDto(request.UsuarioId, request.VueloId));
        return Created(string.Empty, null);
    }

    [HttpDelete]
    public async Task<IActionResult> CancelarSeguimiento(
        [FromBody] CancelarSeguimientoRequest request)
    {
        await _servicio.CancelarAsync(new CancelarSeguimientoDto(request.UsuarioId, request.VueloId));
        return NoContent();
    }

    [HttpGet("usuario/{usuarioId:guid}")]
    public async Task<IActionResult> ObtenerPorUsuario(Guid usuarioId)
    {
        var seguimientos = await _servicio.ObtenerPorUsuarioAsync(usuarioId);
        return Ok(seguimientos);
    }

    [HttpGet("vuelo/{vueloId:guid}")]
    public async Task<IActionResult> ObtenerUsuariosPorVuelo(Guid vueloId)
    {
        var usuarios = await _servicio.ObtenerUsuariosPorVueloAsync(vueloId);
        return Ok(usuarios);
    }
}

public record RegistrarSeguimientoRequest(Guid UsuarioId, Guid VueloId);
public record CancelarSeguimientoRequest(Guid UsuarioId, Guid VueloId);
