using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.Shared.Contracts;

namespace SIV.API.Controllers;

[ApiController]
[Route("api/auditoria")]
[Authorize(Roles = "Auditor,Administrador")] // RNF-SEG-04: el log lo consulta el auditor institucional.
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
