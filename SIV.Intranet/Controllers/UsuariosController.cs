using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.Intranet.Models;
using SIV.Intranet.Services;

namespace SIV.Intranet.Controllers;

/// <summary>
/// Gestión de usuarios internos (CU-USU-03). Todo el controlador es exclusivo del
/// Administrador (RNF-SEG-02): impide la escalada de privilegios. No aplica reglas
/// de negocio por su cuenta; delega en la API, que es la única fuente de verdad.
/// </summary>
[Authorize(Roles = "Administrador")]
public sealed class UsuariosController : Controller
{
    private readonly IUsuariosApi _usuarios;

    public UsuariosController(IUsuariosApi usuarios)
    {
        _usuarios = usuarios;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
        => View(await _usuarios.ObtenerTodosAsync());

    [HttpGet]
    public IActionResult Crear() => View(new CrearUsuarioViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Crear(CrearUsuarioViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var resultado = await _usuarios.CrearInternoAsync(modelo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Exito"] = $"Usuario {modelo.Email} creado con rol {modelo.Rol}.";
        return RedirectToAction(nameof(Index));
    }

    // Elimina una cuenta de forma permanente (borrado definitivo en la API). No se permite
    // que el administrador elimine su propia cuenta, para no quedarse fuera del sistema.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == id.ToString())
        {
            TempData["Error"] = "No puedes eliminar tu propia cuenta.";
            return RedirectToAction(nameof(Index));
        }

        var resultado = await _usuarios.EliminarAsync(id);
        if (resultado.Exito)
            TempData["Exito"] = "Usuario eliminado.";
        else
            TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Index));
    }
}
