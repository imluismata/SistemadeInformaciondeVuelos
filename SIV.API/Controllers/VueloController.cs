using Microsoft.AspNetCore.Mvc;
using SIV.Modules.Vuelos.Application;
using SIV.Modules.Vuelos.Domain;

namespace SIV.API.Controllers;

[ApiController]
[Route("api/vuelos")]
public sealed class VueloController : ControllerBase
{
    private readonly IVueloService _service;

    public VueloController(IVueloService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
        => Ok(await _service.ObtenerTodosAsync());

    [HttpGet("consultar")]
    public async Task<IActionResult> Consultar(
        [FromQuery] Guid? aerolineaId,
        [FromQuery] Guid? aeropuertoOrigenId,
        [FromQuery] Guid? aeropuertoDestinoId,
        [FromQuery] DateTime? fechaDesde,
        [FromQuery] DateTime? fechaHasta,
        [FromQuery] EstadoVuelo? estado)
    {
        var filtro = new ConsultarVuelosQuery(aerolineaId, aeropuertoOrigenId, aeropuertoDestinoId, fechaDesde, fechaHasta, estado);
        return Ok(await _service.ConsultarAsync(filtro));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var vuelo = await _service.ObtenerPorIdAsync(id);
        return vuelo is null ? NotFound() : Ok(vuelo);
    }

    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] RegistrarVueloCommand command)
    {
        var vuelo = await _service.RegistrarAsync(command);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = vuelo.Id }, vuelo);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> ActualizarDatos(Guid id, [FromBody] ActualizarDatosVueloCommand command)
        => Ok(await _service.ActualizarDatosAsync(id, command));

    [HttpPut("{id:guid}/estado")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] ActualizarEstadoVueloCommand command)
        => Ok(await _service.CambiarEstadoAsync(id, command));

    [HttpPost("{id:guid}/cambios-operativos")]
    public async Task<IActionResult> RegistrarCambioOperativo(Guid id, [FromBody] RegistrarCambioOperativoCommand command)
        => Ok(await _service.RegistrarCambioOperativoAsync(id, command));
}