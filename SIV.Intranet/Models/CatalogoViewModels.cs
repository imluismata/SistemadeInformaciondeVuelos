using System.ComponentModel.DataAnnotations;

namespace SIV.Intranet.Models;

/// <summary>
/// Formularios del catálogo. El mismo modelo sirve para crear y editar:
/// si Id viene vacío es alta, si trae valor es modificación (DRY).
/// </summary>
public sealed class AerolineaViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(10, ErrorMessage = "Máximo 10 caracteres.")]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    public bool EsEdicion => Id.HasValue && Id != Guid.Empty;
}

public sealed class AeropuertoViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(10, ErrorMessage = "Máximo 10 caracteres.")]
    [Display(Name = "Código (IATA/ICAO)")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre completo")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El país es obligatorio.")]
    [Display(Name = "País")]
    public string Pais { get; set; } = string.Empty;

    public bool EsEdicion => Id.HasValue && Id != Guid.Empty;
}

/// <summary>
/// Modelo de la pantalla principal del catálogo: ambas colecciones juntas.
/// </summary>
public sealed class CatalogoViewModel
{
    public IReadOnlyList<AerolineaApi> Aerolineas { get; init; } = [];
    public IReadOnlyList<AeropuertoApi> Aeropuertos { get; init; } = [];
}
