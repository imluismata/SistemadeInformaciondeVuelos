using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.Modules.Catalogo.Application;

namespace SIV.API.Controllers;

[ApiController]
[Route("api/catalogo")]
[Authorize] // Consultar el catálogo requiere estar autenticado; escribir requiere Administrador.
public sealed class CatalogoController : ControllerBase
{
    private readonly ICatalogoService _service;

    public CatalogoController(ICatalogoService service)
    {
        _service = service;
    }

    [HttpGet("aerolineas")]
    public async Task<IActionResult> ObtenerAerolineas()
        => Ok(await _service.ObtenerAerolineasAsync());

    [HttpPost("aerolineas")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> RegistrarAerolinea([FromBody] RegistrarAerolineaCommand command)
        => Ok(await _service.RegistrarAerolineaAsync(command));

    [HttpPut("aerolineas/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarAerolinea(Guid id, [FromBody] ActualizarAerolineaCommand command)
        => Ok(await _service.ActualizarAerolineaAsync(id, command));

    [HttpDelete("aerolineas/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DesactivarAerolinea(Guid id)
    {
        await _service.DesactivarAerolineaAsync(id);
        return NoContent();
    }

    [HttpPatch("aerolineas/{id:guid}/activar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ReactivarAerolinea(Guid id)
    {
        await _service.ReactivarAerolineaAsync(id);
        return NoContent();
    }

    [HttpGet("aeropuertos")]
    public async Task<IActionResult> ObtenerAeropuertos()
        => Ok(await _service.ObtenerAeropuertosAsync());

    [HttpPost("aeropuertos")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> RegistrarAeropuerto([FromBody] RegistrarAeropuertoCommand command)
        => Ok(await _service.RegistrarAeropuertoAsync(command));

    [HttpPut("aeropuertos/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarAeropuerto(Guid id, [FromBody] ActualizarAeropuertoCommand command)
        => Ok(await _service.ActualizarAeropuertoAsync(id, command));

    [HttpDelete("aeropuertos/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DesactivarAeropuerto(Guid id)
    {
        await _service.DesactivarAeropuertoAsync(id);
        return NoContent();
    }

    [HttpPatch("aeropuertos/{id:guid}/activar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ReactivarAeropuerto(Guid id)
    {
        await _service.ReactivarAeropuertoAsync(id);
        return NoContent();
    }

    // ==================== Terminales ====================
    [HttpGet("terminales")]
    public async Task<IActionResult> ObtenerTerminales()
        => Ok(await _service.ObtenerTerminalesAsync());

    [HttpPost("terminales")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> RegistrarTerminal([FromBody] RegistrarTerminalCommand command)
        => Ok(await _service.RegistrarTerminalAsync(command));

    [HttpPut("terminales/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarTerminal(Guid id, [FromBody] ActualizarTerminalCommand command)
        => Ok(await _service.ActualizarTerminalAsync(id, command));

    [HttpDelete("terminales/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DesactivarTerminal(Guid id)
    {
        await _service.DesactivarTerminalAsync(id);
        return NoContent();
    }

    [HttpPatch("terminales/{id:guid}/activar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ReactivarTerminal(Guid id)
    {
        await _service.ReactivarTerminalAsync(id);
        return NoContent();
    }

    // ==================== Puertas ====================
    [HttpGet("puertas")]
    public async Task<IActionResult> ObtenerPuertas()
        => Ok(await _service.ObtenerPuertasAsync());

    [HttpPost("puertas")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> RegistrarPuerta([FromBody] RegistrarPuertaCommand command)
        => Ok(await _service.RegistrarPuertaAsync(command));

    [HttpPut("puertas/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ActualizarPuerta(Guid id, [FromBody] ActualizarPuertaCommand command)
        => Ok(await _service.ActualizarPuertaAsync(id, command));

    [HttpDelete("puertas/{id:guid}")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> DesactivarPuerta(Guid id)
    {
        await _service.DesactivarPuertaAsync(id);
        return NoContent();
    }

    [HttpPatch("puertas/{id:guid}/activar")]
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> ReactivarPuerta(Guid id)
    {
        await _service.ReactivarPuertaAsync(id);
        return NoContent();
    }
}