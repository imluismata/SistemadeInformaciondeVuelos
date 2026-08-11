using System.ComponentModel.DataAnnotations;

namespace SIV.Intranet.Models;

/// <summary>
/// Datos del formulario para registrar un cambio operativo sobre un vuelo.
/// </summary>
public sealed class CambioOperativoViewModel
{
    public Guid VueloId { get; set; }

    [Required(ErrorMessage = "Selecciona el tipo de cambio.")]
    [Display(Name = "Tipo de cambio")]
    public string Tipo { get; set; } = "Retraso";

    [Required(ErrorMessage = "El motivo es obligatorio.")]
    [Display(Name = "Motivo")]
    public string Motivo { get; set; } = string.Empty;

    [Display(Name = "Duración (hh:mm)")]
    public string? Duracion { get; set; }

    [Display(Name = "Nueva puerta")]
    public Guid? NuevaPuertaId { get; set; }

    public static IReadOnlyList<string> TiposDisponibles { get; } =
        ["Retraso", "Adelanto", "CambioDePuerta", "Cancelacion"];
}
