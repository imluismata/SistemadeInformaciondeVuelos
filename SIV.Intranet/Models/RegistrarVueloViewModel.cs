using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SIV.Intranet.Models;

/// <summary>
/// Datos del formulario de registro de vuelo. Las listas desplegables
/// se llenan desde el catálogo de la API.
/// </summary>
public sealed class RegistrarVueloViewModel
{
    [Required(ErrorMessage = "El número de vuelo es obligatorio.")]
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
    public DateTime HorarioSalida { get; set; } = DateTime.Today.AddHours(8);

    [Required(ErrorMessage = "El horario de llegada es obligatorio.")]
    [Display(Name = "Llegada")]
    [DataType(DataType.DateTime)]
    public DateTime HorarioLlegada { get; set; } = DateTime.Today.AddHours(11);

    // La puerta se elige del catálogo (opcional). Se guarda su Id; el backend
    // resuelve y almacena la descripción ("A5 · Terminal A").
    [Display(Name = "Puerta")]
    public Guid? PuertaId { get; set; }

    // Opciones para los <select>; no forman parte de los datos enviados.
    public IEnumerable<SelectListItem> Aerolineas { get; set; } = [];
    public IEnumerable<SelectListItem> Aeropuertos { get; set; } = [];
    public IEnumerable<SelectListItem> Puertas { get; set; } = [];
}
