using System.ComponentModel.DataAnnotations;

namespace SIV.Intranet.Models;

// Modelos espejo de los DTOs de reporte que devuelve la API.
public sealed record ConteoApi(string Clave, int Cantidad);

public sealed record ReporteOperacionApi(
    DateTime? Desde, DateTime? Hasta, int Total, IReadOnlyList<ConteoApi> PorEstado);

public sealed record CambioReporteApi(
    DateTime RegistradoEn, string Vuelo, string Tipo, string Motivo);

public sealed record ReporteCambiosApi(
    DateTime? Desde, DateTime? Hasta, int Total,
    IReadOnlyList<ConteoApi> PorTipo, IReadOnlyList<CambioReporteApi> Detalle);

public sealed record VueloSeguidoApi(string Vuelo, int Seguidores);

public sealed record ReporteSeguimientoApi(
    int TotalSeguimientosActivos, IReadOnlyList<VueloSeguidoApi> VuelosMasSeguidos);

// Pantalla de reportes: filtros de período + los dos reportes ya resueltos.
public sealed class ReportesViewModel
{
    [Display(Name = "Desde")]
    [DataType(DataType.Date)]
    public DateTime? Desde { get; set; }

    [Display(Name = "Hasta")]
    [DataType(DataType.Date)]
    public DateTime? Hasta { get; set; }

    public ReporteOperacionApi? Operacion { get; set; }
    public ReporteCambiosApi? Cambios { get; set; }
    public ReporteSeguimientoApi? Seguimiento { get; set; }
}
