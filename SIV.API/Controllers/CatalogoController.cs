using Microsoft.AspNetCore.Mvc;
using SIV.Modules.Catalogo.Application;

namespace SIV.API.Controllers;

[ApiController]
[Route("api/catalogo")]
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
    public async Task<IActionResult> RegistrarAerolinea([FromBody] RegistrarAerolineaCommand command)
        => Ok(await _service.RegistrarAerolineaAsync(command));

    [HttpPut("aerolineas/{id:guid}")]
    public async Task<IActionResult> ActualizarAerolinea(Guid id, [FromBody] ActualizarAerolineaCommand command)
        => Ok(await _service.ActualizarAerolineaAsync(id, command));

    [HttpDelete("aerolineas/{id:guid}")]
    public async Task<IActionResult> DesactivarAerolinea(Guid id)
    {
        await _service.DesactivarAerolineaAsync(id);
        return NoContent();
    }

    [HttpGet("aeropuertos")]
    public async Task<IActionResult> ObtenerAeropuertos()
        => Ok(await _service.ObtenerAeropuertosAsync());

    [HttpPost("aeropuertos")]
    public async Task<IActionResult> RegistrarAeropuerto([FromBody] RegistrarAeropuertoCommand command)
        => Ok(await _service.RegistrarAeropuertoAsync(command));

    [HttpPut("aeropuertos/{id:guid}")]
    public async Task<IActionResult> ActualizarAeropuerto(Guid id, [FromBody] ActualizarAeropuertoCommand command)
        => Ok(await _service.ActualizarAeropuertoAsync(id, command));

    [HttpDelete("aeropuertos/{id:guid}")]
    public async Task<IActionResult> DesactivarAeropuerto(Guid id)
    {
        await _service.DesactivarAeropuertoAsync(id);
        return NoContent();
    }
}