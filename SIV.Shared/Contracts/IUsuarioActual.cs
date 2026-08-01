namespace SIV.Shared.Contracts;

/// <summary>
/// Abstracción que expone quién es el usuario que está ejecutando la petición
/// actual, sin acoplar la capa de aplicación al HttpContext ni a ASP.NET.
///
/// La implementación vive en la capa API (lee los claims del JWT vía
/// IHttpContextAccessor). La consume AuditoriaService para registrar el "actor"
/// de cada acción (RNF-TRZ), de modo que ningún otro servicio tiene que pasar el
/// usuario a mano. Cuando no hay petición HTTP (por ejemplo, la siembra en el
/// arranque), devuelve "Sistema".
/// </summary>
public interface IUsuarioActual
{
    /// <summary>Identificación legible del actor: su email, o "Sistema" si no hay usuario autenticado.</summary>
    string Descripcion { get; }
}
