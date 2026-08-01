using System.ComponentModel.DataAnnotations;

namespace SIV.Intranet.Models;

/// <summary>
/// Registro del log de auditoría tal como lo devuelve la API.
/// </summary>
public sealed record AuditoriaApi(
    Guid Id,
    string Modulo,
    string Accion,
    string? Detalle,
    string Resultado,
    string Actor,
    DateTime FechaHora);

/// <summary>
/// Filtros de consulta del log. Todos son opcionales.
/// El log es de solo lectura (RNF-SEG-04): no existen operaciones de escritura.
/// </summary>
public sealed class AuditoriaFiltroViewModel
{
    [Display(Name = "Módulo")]
    public string? Modulo { get; set; }

    [Display(Name = "Acción")]
    public string? Accion { get; set; }

    [Display(Name = "Desde")]
    [DataType(DataType.Date)]
    public DateTime? Desde { get; set; }

    [Display(Name = "Hasta")]
    [DataType(DataType.Date)]
    public DateTime? Hasta { get; set; }

    public IReadOnlyList<AuditoriaApi> Registros { get; set; } = [];

    /// <summary>Módulos del sistema, para el desplegable de filtro.</summary>
    public static IReadOnlyList<string> Modulos { get; } =
        ["Vuelos", "Estados", "CambiosOperativos", "Catalogo", "Usuarios"];
}
