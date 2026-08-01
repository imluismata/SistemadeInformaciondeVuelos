using System.ComponentModel.DataAnnotations;

namespace SIV.Intranet.Models;

// Usuario tal como lo devuelve GET /api/usuarios.
public sealed record UsuarioApi(Guid Id, string Nombre, string Email, string Rol, DateTime CreadoEn);

// Formulario de alta de personal interno (solo Administrador). El rol se asigna
// de entrada y el admin define una contraseña inicial que el empleado usará.
public sealed class CrearUsuarioViewModel
{
    // Roles que un administrador puede asignar desde la intranet.
    public static readonly IReadOnlyList<string> RolesDisponibles =
        new[] { "OperadorVuelos", "Auditor", "Administrador" };

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Entre 2 y 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [Display(Name = "Correo")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña inicial es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Mínimo 8 caracteres.")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña inicial")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "El rol es obligatorio.")]
    [Display(Name = "Rol")]
    public string Rol { get; set; } = "OperadorVuelos";
}
