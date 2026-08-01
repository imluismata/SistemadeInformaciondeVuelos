using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.Intranet.Models;
using SIV.Intranet.Services;

namespace SIV.Intranet.Controllers;

/// <summary>
/// Reportes operativos (Módulo 9). Reservado a Administrador y Auditor (RNF-SEG-04),
/// igual que en la API. Solo orquesta: pide los reportes al cliente de la API y
/// entrega las descargas CSV que la API genera.
/// </summary>
[Authorize(Roles = "Auditor,Administrador")]
public sealed class ReportesController : Controller
{
    private readonly IReportesApi _reportes;

    public ReportesController(IReportesApi reportes)
    {
        _reportes = reportes;
    }

    [HttpGet]
    public async Task<IActionResult> Index(DateTime? desde, DateTime? hasta)
    {
        var modelo = new ReportesViewModel
        {
            Desde = desde,
            Hasta = hasta,
            Operacion = await _reportes.OperacionAsync(desde, hasta),
            Cambios = await _reportes.CambiosAsync(desde, hasta),
            Seguimiento = await _reportes.SeguimientoAsync()
        };

        return View(modelo);
    }

    [HttpGet]
    public async Task<IActionResult> SeguimientoCsv()
    {
        var bytes = await _reportes.SeguimientoCsvAsync();
        return File(bytes, "text/csv", $"reporte-seguimiento-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> OperacionCsv(DateTime? desde, DateTime? hasta)
    {
        var bytes = await _reportes.OperacionCsvAsync(desde, hasta);
        return File(bytes, "text/csv", $"reporte-operacion-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }

    [HttpGet]
    public async Task<IActionResult> CambiosCsv(DateTime? desde, DateTime? hasta)
    {
        var bytes = await _reportes.CambiosCsvAsync(desde, hasta);
        return File(bytes, "text/csv", $"reporte-cambios-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }
}
