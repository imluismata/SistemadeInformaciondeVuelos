using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.Modules.Reportes.Application;
using SIV.Shared.DTOs;

namespace SIV.API.Controllers;

/// <summary>
/// Reportes operativos (Módulo 9). Consulta y exportación reservadas al
/// Administrador y al Auditor, igual que el log de auditoría (RNF-SEG-04).
/// El controlador solo orquesta y da formato (JSON o CSV); la agregación la
/// hace el módulo de Reportes.
/// </summary>
[ApiController]
[Route("api/reportes")]
[Authorize(Roles = "Auditor,Administrador")]
public sealed class ReportesController(IReporteService reportes) : ControllerBase
{
    // CU-REP-01
    [HttpGet("operacion")]
    public async Task<IActionResult> Operacion([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        => Ok(await reportes.GenerarOperacionAsync(desde, hasta));

    // CU-REP-02
    [HttpGet("cambios")]
    public async Task<IActionResult> Cambios([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        => Ok(await reportes.GenerarCambiosAsync(desde, hasta));

    // CU-REP-03
    [HttpGet("seguimiento")]
    public async Task<IActionResult> Seguimiento()
        => Ok(await reportes.GenerarSeguimientoAsync());

    // CU-REP-05: exportación del reporte de seguimiento en CSV.
    [HttpGet("seguimiento/csv")]
    public async Task<IActionResult> SeguimientoCsv()
    {
        var reporte = await reportes.GenerarSeguimientoAsync();

        var csv = new StringBuilder();
        csv.AppendLine("Vuelo,Seguidores");
        foreach (var v in reporte.VuelosMasSeguidos)
            csv.AppendLine($"{Escapar(v.Vuelo)},{v.Seguidores}");
        csv.AppendLine($"Total,{reporte.TotalSeguimientosActivos}");

        return ArchivoCsv(csv, "reporte-seguimiento");
    }

    // CU-REP-05: exportación del reporte de operación en CSV.
    [HttpGet("operacion/csv")]
    public async Task<IActionResult> OperacionCsv([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var reporte = await reportes.GenerarOperacionAsync(desde, hasta);

        var csv = new StringBuilder();
        csv.AppendLine("Estado,Cantidad");
        foreach (var fila in reporte.PorEstado)
            csv.AppendLine($"{Escapar(fila.Clave)},{fila.Cantidad}");
        csv.AppendLine($"Total,{reporte.Total}");

        return ArchivoCsv(csv, "reporte-operacion");
    }

    // CU-REP-05: exportación del reporte de cambios en CSV.
    [HttpGet("cambios/csv")]
    public async Task<IActionResult> CambiosCsv([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var reporte = await reportes.GenerarCambiosAsync(desde, hasta);

        var csv = new StringBuilder();
        csv.AppendLine("Fecha,Vuelo,Tipo,Motivo");
        foreach (var c in reporte.Detalle)
            csv.AppendLine($"{c.RegistradoEn:yyyy-MM-dd HH:mm},{Escapar(c.Vuelo)},{Escapar(c.Tipo)},{Escapar(c.Motivo)}");

        return ArchivoCsv(csv, "reporte-cambios");
    }

    private FileContentResult ArchivoCsv(StringBuilder csv, string nombre)
    {
        // BOM UTF-8 para que Excel muestre bien los acentos.
        var bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes(csv.ToString())).ToArray();
        return File(bytes, "text/csv", $"{nombre}-{DateTime.Now:yyyyMMdd-HHmm}.csv");
    }

    // Envuelve en comillas y escapa las internas si el valor trae comas o comillas.
    private static string Escapar(string valor)
        => valor.Contains(',') || valor.Contains('"')
            ? $"\"{valor.Replace("\"", "\"\"")}\""
            : valor;
}
