using SIV.Shared.DTOs;

namespace SIV.Modules.Reportes.Application;

/// <summary>
/// Servicio de reportes operativos (Módulo 9 del SAD). Genera vistas agregadas a
/// partir de los datos de otros módulos, a los que accede solo por contratos de
/// SIV.Shared (DA-02). De momento cubre operación (CU-REP-01) y cambios (CU-REP-02).
/// </summary>
public interface IReporteService
{
    Task<ReporteOperacionDto> GenerarOperacionAsync(DateTime? desde, DateTime? hasta);
    Task<ReporteCambiosDto> GenerarCambiosAsync(DateTime? desde, DateTime? hasta);
    Task<ReporteSeguimientoDto> GenerarSeguimientoAsync();
}
