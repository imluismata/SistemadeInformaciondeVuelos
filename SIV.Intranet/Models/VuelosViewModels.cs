using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIV.Intranet.Models;

/// <summary>
/// Formulario de edición de los datos de un vuelo (CU-VUE-02). El número no se
/// edita (identifica el vuelo); el motivo del cambio es obligatorio y queda
/// registrado por el backend de forma inmutable.
/// </summary>
public sealed class EditarVueloViewModel
{
    public Guid Id { get; set; }

    [Display(Name = "Número de vuelo")]
    public string Numero { get; set; } = string.Empty;

    [Required(ErrorMessage = "Selecciona una aerolínea.")]
    [Display(Name = "Aerolínea")]
    public Guid AerolineaId { get; set; }

    [Required(ErrorMessage = "Selecciona el aeropuerto de origen.")]
    [Display(Name = "Origen")]
    public Guid AeropuertoOrigenId { get; set; }

    [Required(ErrorMessage = "Selecciona el aeropuerto de destino.")]
    [Display(Name = "Destino")]
    public Guid AeropuertoDestinoId { get; set; }

    [Required(ErrorMessage = "El horario de salida es obligatorio.")]
    [Display(Name = "Salida")]
    [DataType(DataType.DateTime)]
    public DateTime HorarioSalida { get; set; }

    [Required(ErrorMessage = "El horario de llegada es obligatorio.")]
    [Display(Name = "Llegada")]
    [DataType(DataType.DateTime)]
    public DateTime HorarioLlegada { get; set; }

    [Display(Name = "Puerta")]
    public Guid? PuertaId { get; set; }

    [Required(ErrorMessage = "El motivo del cambio es obligatorio.")]
    [Display(Name = "Motivo del cambio")]
    public string Motivo { get; set; } = string.Empty;

    public IEnumerable<SelectListItem> Aerolineas { get; set; } = [];
    public IEnumerable<SelectListItem> Aeropuertos { get; set; } = [];
    public IEnumerable<SelectListItem> Puertas { get; set; } = [];
}

/// <summary>
/// Filtros de consulta de vuelos (CU-VUE-03). Todos opcionales. Incluye los
/// resultados y el catálogo de aerolíneas para el desplegable.
/// </summary>
public sealed class VuelosFiltroViewModel
{
    [Display(Name = "Aerolínea")]
    public Guid? AerolineaId { get; set; }

    [Display(Name = "Estado")]
    public string? Estado { get; set; }

    [Display(Name = "Desde")]
    [DataType(DataType.Date)]
    public DateTime? FechaDesde { get; set; }

    [Display(Name = "Hasta")]
    [DataType(DataType.Date)]
    public DateTime? FechaHasta { get; set; }

    public IReadOnlyList<VueloApi> Vuelos { get; set; } = [];
    public IEnumerable<SelectListItem> Aerolineas { get; set; } = [];

    // ¿El usuario aplicó algún filtro? Para decidir entre consultar y traer todos.
    public bool HayFiltro => AerolineaId.HasValue || !string.IsNullOrWhiteSpace(Estado)
                             || FechaDesde.HasValue || FechaHasta.HasValue;
}
