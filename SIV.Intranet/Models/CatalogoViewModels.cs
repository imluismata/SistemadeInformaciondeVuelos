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
/// Formulario de terminal. Una terminal pertenece a un aeropuerto (AILA tiene
/// dos: A y B). El mismo modelo sirve para alta y edición.
/// </summary>
public sealed class TerminalViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(10, ErrorMessage = "Máximo 10 caracteres.")]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona el aeropuerto.")]
    [Display(Name = "Aeropuerto")]
    public Guid AeropuertoId { get; set; }

    // Opciones para el desplegable de aeropuertos.
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Aeropuertos { get; set; } = [];

    public bool EsEdicion => Id.HasValue && Id != Guid.Empty;
}

/// <summary>
/// Formulario de puerta. Si no se elige terminal, la puerta es una rampa abierta
/// (posición remota). El mismo modelo sirve para alta y edición.
/// </summary>
public sealed class PuertaViewModel
{
    public Guid? Id { get; set; }

    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(10, ErrorMessage = "Máximo 10 caracteres.")]
    [Display(Name = "Código")]
    public string Codigo { get; set; } = string.Empty;

    [Display(Name = "Terminal")]
    public Guid? TerminalId { get; set; }

    // Opciones para el desplegable de terminales.
    public IEnumerable<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem> Terminales { get; set; } = [];

    public bool EsEdicion => Id.HasValue && Id != Guid.Empty;
}

/// <summary>
/// Modelo de la pantalla principal del catálogo: todas las colecciones juntas.
/// </summary>
public sealed class CatalogoViewModel
{
    public IReadOnlyList<AerolineaApi> Aerolineas { get; init; } = [];
    public IReadOnlyList<AeropuertoApi> Aeropuertos { get; init; } = [];
    public IReadOnlyList<TerminalApi> Terminales { get; init; } = [];
    public IReadOnlyList<PuertaApi> Puertas { get; init; } = [];
}
