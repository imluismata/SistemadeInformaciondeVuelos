using Microsoft.AspNetCore.Mvc;

namespace SIV.Intranet.Controllers;

/// <summary>
/// Páginas de error con la marca del sistema. Las alimenta el pipeline:
/// UseStatusCodePagesWithReExecute (404, 403…), UseExceptionHandler (500) y la
/// ruta de acceso denegado de la cookie. Cada vista es auto-contenida (sin layout,
/// CSS y SVG inline) para que se muestre aunque la app esté en mal estado.
/// </summary>
[Route("Error")]
public sealed class ErrorController : Controller
{
    [Route("{codigo:int}")]
    public IActionResult Index(int codigo)
    {
        Response.StatusCode = codigo;

        return codigo switch
        {
            403 => View("AccesoDenegado"),
            404 => View("NoEncontrado"),
            _ => View("ErrorInterno", codigo) // 500 y cualquier otro no previsto
        };
    }
}
