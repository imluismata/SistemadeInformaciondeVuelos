using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.Modules.Vuelos.Application;
using SIV.Shared.DTOs;
using SIV.Shared.Enums;

namespace SIV.API.Controllers;

[ApiController]
[Route("api/vuelos")]
[Authorize] // Todo endpoint de vuelos requiere estar autenticado.
public sealed class VueloController : ControllerBase
{
    private readonly IVueloService _service;
    private readonly IVueloImportacionService _importacion;

    public VueloController(IVueloService service, IVueloImportacionService importacion)
    {
        _service = service;
        _importacion = importacion;
    }

    // Si me mandan un 'tamano', devuelvo solo esa pagina; si no, la lista completa
    // como antes (para no romper lo que ya la usaba).
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int? pagina, [FromQuery] int? tamano)
    {
        if (tamano is > 0)
            return Ok(await _service.ObtenerPaginadoAsync(pagina ?? 1, tamano.Value));

        return Ok(await _service.ObtenerTodosAsync());
    }

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
    [Authorize(Roles = "OperadorVuelos,Administrador")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarVueloCommand command)
    {
        var vuelo = await _service.RegistrarAsync(command);
        return CreatedAtAction(nameof(ObtenerPorId), new { id = vuelo.Id }, vuelo);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "OperadorVuelos,Administrador")]
    public async Task<IActionResult> ActualizarDatos(Guid id, [FromBody] ActualizarDatosVueloCommand command)
        => Ok(await _service.ActualizarDatosAsync(id, command));

    [HttpPut("{id:guid}/estado")]
    [Authorize(Roles = "OperadorVuelos,Administrador")]
    public async Task<IActionResult> CambiarEstado(Guid id, [FromBody] ActualizarEstadoVueloCommand command)
        => Ok(await _service.CambiarEstadoAsync(id, command));

    [HttpPost("{id:guid}/cambios-operativos")]
    [Authorize(Roles = "OperadorVuelos,Administrador")]
    public async Task<IActionResult> RegistrarCambioOperativo(Guid id, [FromBody] RegistrarCambioOperativoCommand command)
        => Ok(await _service.RegistrarCambioOperativoAsync(id, command));

    // Importación masiva desde CSV/Excel en tres pasos: el archivo se lee una vez
    // (previsualizar) y luego se trabaja sobre las filas —ya editables— para revalidar
    // (validar) e importar. Todas devuelven el resultado fila por fila.
    [HttpPost("importacion/previsualizar")]
    [Authorize(Roles = "OperadorVuelos,Administrador")]
    public async Task<IActionResult> PrevisualizarImportacion(IFormFile archivo)
    {
        if (archivo is null || archivo.Length == 0)
            return BadRequest(new { error = "Debes adjuntar un archivo .csv o .xlsx." });

        await using var flujo = archivo.OpenReadStream();
        return Ok(await _importacion.PrevisualizarArchivoAsync(flujo, archivo.FileName));
    }

    [HttpPost("importacion/validar")]
    [Authorize(Roles = "OperadorVuelos,Administrador")]
    public async Task<IActionResult> ValidarImportacion([FromBody] List<FilaVueloImportacion> filas)
        => Ok(await _importacion.ValidarFilasAsync(filas));

    [HttpPost("importacion")]
    [Authorize(Roles = "OperadorVuelos,Administrador")]
    public async Task<IActionResult> Importar([FromBody] List<FilaVueloImportacion> filas)
        => Ok(await _importacion.ImportarFilasAsync(filas));
}