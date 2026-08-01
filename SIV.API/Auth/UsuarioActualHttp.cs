using System.Security.Claims;
using SIV.Shared.Contracts;

namespace SIV.API.Auth;

/// <summary>
/// Implementación de <see cref="IUsuarioActual"/> para la Web API: obtiene el
/// actor de los claims del JWT de la petición en curso. Si no hay usuario
/// autenticado (petición anónima o tareas del arranque), devuelve "Sistema".
/// </summary>
public sealed class UsuarioActualHttp : IUsuarioActual
{
    private readonly IHttpContextAccessor _contexto;

    public UsuarioActualHttp(IHttpContextAccessor contexto)
    {
        _contexto = contexto;
    }

    public string Descripcion
    {
        get
        {
            var usuario = _contexto.HttpContext?.User;
            if (usuario?.Identity?.IsAuthenticated != true)
                return "Sistema";

            // El email viaja como claim en el token; se cae al nombre si faltara.
            return usuario.FindFirstValue(ClaimTypes.Email)
                   ?? usuario.FindFirstValue("email")
                   ?? usuario.FindFirstValue(ClaimTypes.Name)
                   ?? "Desconocido";
        }
    }
}
