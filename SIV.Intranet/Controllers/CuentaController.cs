using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using SIV.Intranet.Models;
using SIV.Intranet.Services;

namespace SIV.Intranet.Controllers;

/// <summary>
/// Gestiona el inicio y cierre de sesión de la intranet. No valida
/// credenciales por su cuenta: delega en la API y, con la respuesta,
/// crea la cookie de sesión que protege las vistas.
/// </summary>
public sealed class CuentaController : Controller
{
    private readonly IAutenticacionApi _api;

    public CuentaController(IAutenticacionApi api)
    {
        _api = api;
    }

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var resultado = await _api.LoginAsync(modelo.Email, modelo.Password);
        if (resultado is null)
        {
            ModelState.AddModelError(string.Empty, "Correo o contraseña incorrectos.");
            return View(modelo);
        }

        // La identidad de la intranet guarda nombre, rol y el token JWT
        // (este último para reenviarlo a la API en cada petición).
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, resultado.Usuario.Id.ToString()),
            new(ClaimTypes.Name, resultado.Usuario.Nombre),
            new(ClaimTypes.Role, resultado.Usuario.Rol),
            new(TokenHandler.ClaimToken, resultado.Token)
        };

        var identidad = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identidad));

        return RedirectToAction("Index", "Vuelos");
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Login");
    }

    [HttpGet]
    public IActionResult Denegado() => View();
}
