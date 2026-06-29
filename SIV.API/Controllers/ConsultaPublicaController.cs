using Microsoft.AspNetCore.Mvc;
using SIV.Modules.ConsultaPublica.Application;

namespace SIV.API.Controllers;

// endpoints publicos, cualquier persona puede consultar sin autenticarse
[ApiController]
[Route("api/vuelos")]
public class ConsultaPublicaController : ControllerBase
{
    private readonly IConsultaPublicaService _servicio;

    public ConsultaPublicaController(IConsultaPublicaService servicio)
    {
        _servicio = servicio;
    }

    // lista todos los vuelos activos del dia
    [HttpGet]
    public async Task<IActionResult> ObtenerVuelosActivos()
    {
        var vuelos = await _servicio.ObtenerVuelosActivosAsync();
        return Ok(vuelos);
    }

    // busca un vuelo por su numero, ej: AA123
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

    // retorna el detalle completo de un vuelo por su id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerDetalle(Guid id)
    {
        var vuelo = await _servicio.ObtenerDetallePorIdAsync(id);

        if (vuelo is null)
            return NotFound($"No se encontró el vuelo con Id {id}.");

        return Ok(vuelo);
    }
}
