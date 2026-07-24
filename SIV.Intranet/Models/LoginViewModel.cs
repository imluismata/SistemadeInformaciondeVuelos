using System.ComponentModel.DataAnnotations;

namespace SIV.Intranet.Models;

/// <summary>
/// Datos que el usuario ingresa en el formulario de login de la intranet.
/// </summary>
public sealed class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
