using Microsoft.AspNetCore.Mvc;
using SIV.Modules.ConsultaPublica.Application.Dtos;
using SIV.Modules.ConsultaPublica.Application.Interfaces;

namespace SIV.API.Controllers;

[ApiController]
[Route("api/vuelos")]
public class ConsultaPublicaController : ControllerBase
{
    private readonly IConsultaPublicaService _servicio;

    public ConsultaPublicaController(IConsultaPublicaService servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerVuelosActivos()
    {
        var vuelos = await _servicio.ObtenerVuelosActivosAsync();
        return Ok(vuelos);
    }

    [HttpGet("buscar")]
    public async Task<IActionResult> BuscarPorNumero([FromQuery] string numero)
    {
        if (string.IsNullOrWhiteSpace(numero))
            return BadRequest("El número de vuelo es obligatorio.");

        var vuelo = await _servicio.BuscarPorNumeroAsync(numero);

        if (vuelo is null)
            return NotFound($"No se encontró un vuelo con el número {numero}.");

        return Ok(vuelo);
    }

    [HttpGet("filtro")]
    public async Task<IActionResult> BuscarConFiltro(
        [FromQuery] string? origen,
        [FromQuery] string? destino,
        [FromQuery] DateTime? fecha)
    {
        var filtro = new FiltroConsultaDto(origen, destino, fecha);
        var vuelos = await _servicio.BuscarConFiltroAsync(filtro);
        return Ok(vuelos);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerDetalle(Guid id)
    {
        var vuelo = await _servicio.ObtenerDetallePorIdAsync(id);

        if (vuelo is null)
            return NotFound($"No se encontró el vuelo con Id {id}.");

        return Ok(vuelo);
    }
}
