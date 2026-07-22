using SIV.Modules.Usuarios.Application.Dtos;

namespace SIV.API.Auth;

/// <summary>
/// Abstracción para emitir un token JWT a partir de un usuario ya autenticado.
/// Vive en el borde de la API porque la emisión del token es una preocupación
/// de seguridad de transporte, no lógica de dominio.
/// </summary>
public interface IProveedorTokenJwt
{
    string GenerarToken(UsuarioDto usuario);
}
