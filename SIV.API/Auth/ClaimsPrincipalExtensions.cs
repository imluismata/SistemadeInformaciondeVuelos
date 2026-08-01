using System.Security.Claims;

namespace SIV.API.Auth;

/// <summary>
/// Ayudantes para leer la identidad del usuario autenticado desde los claims del
/// token JWT. Centraliza la lógica para que los controladores no repitan la forma
/// de extraer el Id ('sub') ni la comprobación de rol.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    // El Id del usuario viaja en el claim 'sub'; el manejo por defecto de JWT lo
    // mapea a ClaimTypes.NameIdentifier. Se comprueban ambos por robustez.
    public static Guid? ObtenerUsuarioId(this ClaimsPrincipal user)
    {
        var valor = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return Guid.TryParse(valor, out var id) ? id : null;
    }

    public static bool EsAdministrador(this ClaimsPrincipal user) => user.IsInRole("Administrador");
}
