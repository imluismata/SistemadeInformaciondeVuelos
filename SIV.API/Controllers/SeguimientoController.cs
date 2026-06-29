using Microsoft.AspNetCore.Mvc;
using SIV.Modules.Seguimiento.Application;

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

    // el usuario se suscribe a un vuelo para recibir notificaciones
    [HttpPost]
    public async Task<IActionResult> RegistrarSeguimiento(
        [FromBody] RegistrarSeguimientoRequest request)
    {
        await _servicio.RegistrarSeguimientoAsync(request.UsuarioId, request.VueloId);
        return Created(string.Empty, null);
    }

    // el usuario cancela el seguimiento de un vuelo
    [HttpDelete]
    public async Task<IActionResult> CancelarSeguimiento(
        [FromBody] CancelarSeguimientoRequest request)
    {
        await _servicio.CancelarSeguimientoAsync(request.UsuarioId, request.VueloId);
        return NoContent();
    }

    // retorna los usuarios que estan siguiendo un vuelo
    [HttpGet("vuelo/{vueloId:guid}")]
    public async Task<IActionResult> ObtenerUsuariosPorVuelo(Guid vueloId)
    {
        var usuarios = await _servicio.ObtenerUsuariosPorVueloAsync(vueloId);
        return Ok(usuarios);
    }
}

public record RegistrarSeguimientoRequest(Guid UsuarioId, Guid VueloId);
public record CancelarSeguimientoRequest(Guid UsuarioId, Guid VueloId);
