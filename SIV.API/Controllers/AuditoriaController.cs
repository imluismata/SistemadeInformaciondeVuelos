using Microsoft.AspNetCore.Mvc;
using SIV.Modules.Auditoria.Application;

namespace SIV.API.Controllers;

[ApiController]
[Route("api/auditoria")]
public sealed class AuditoriaController(IAuditoriaService service) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Consultar(
        [FromQuery] string? modulo,
        [FromQuery] string? accion,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta)
        => Ok(await service.ConsultarAsync(modulo, accion, desde, hasta));
}
