using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.Intranet.Models;
using SIV.Intranet.Services;

namespace SIV.Intranet.Controllers;

/// <summary>
/// Actividad de usuarios: historial de seguimientos (CU-SEG-04) y registro de
/// notificaciones (CU-NOT-04). Reservado a Administrador y Auditor (RNF-SEG-04 /
/// RNF-TRZ-03). Solo lectura; delega en la API.
/// </summary>
[Authorize(Roles = "Auditor,Administrador")]
public sealed class ActividadController : Controller
{
    private readonly IActividadApi _actividad;

    public ActividadController(IActividadApi actividad)
    {
        _actividad = actividad;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var modelo = new ActividadViewModel
        {
            Seguimientos = await _actividad.SeguimientosAsync(),
            Notificaciones = await _actividad.NotificacionesAsync()
        };

        return View(modelo);
    }
}
