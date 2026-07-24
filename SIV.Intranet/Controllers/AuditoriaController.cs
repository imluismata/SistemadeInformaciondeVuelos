using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.Intranet.Models;
using SIV.Intranet.Services;

namespace SIV.Intranet.Controllers;

/// <summary>
/// Consulta del log de auditoría. Solo lectura por diseño (RNF-SEG-04):
/// los registros no pueden modificarse ni eliminarse desde ninguna capa.
/// Restringido al auditor institucional y al administrador.
/// </summary>
[Authorize(Roles = "Auditor,Administrador")]
public sealed class AuditoriaController : Controller
{
    private readonly IAuditoriaApi _auditoria;

    public AuditoriaController(IAuditoriaApi auditoria)
    {
        _auditoria = auditoria;
    }

    [HttpGet]
    public async Task<IActionResult> Index(AuditoriaFiltroViewModel filtro)
    {
        filtro.Registros = await _auditoria.ConsultarAsync(
            filtro.Modulo, filtro.Accion, filtro.Desde, filtro.Hasta);

        return View(filtro);
    }
}
