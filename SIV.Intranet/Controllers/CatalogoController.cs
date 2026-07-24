using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.Intranet.Models;
using SIV.Intranet.Services;

namespace SIV.Intranet.Controllers;

/// <summary>
/// Administración del catálogo aeroportuario (aerolíneas y aeropuertos).
/// Consultar requiere sesión; modificar requiere rol Administrador, igual
/// que en la API. Las reglas (código único, no eliminar con vuelos activos)
/// las aplica el módulo de Catálogo, no esta capa.
/// </summary>
[Authorize]
public sealed class CatalogoController : Controller
{
    private const string RolAdmin = "Administrador";

    private readonly ICatalogoApi _catalogo;

    public CatalogoController(ICatalogoApi catalogo)
    {
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var modelo = new CatalogoViewModel
        {
            Aerolineas = await _catalogo.ObtenerAerolineasAsync(),
            Aeropuertos = await _catalogo.ObtenerAeropuertosAsync()
        };

        return View(modelo);
    }

    // ---------- Aerolíneas ----------

    [HttpGet]
    [Authorize(Roles = RolAdmin)]
    public async Task<IActionResult> Aerolinea(Guid? id)
    {
        if (id is null)
            return View(new AerolineaViewModel());

        var existente = (await _catalogo.ObtenerAerolineasAsync()).FirstOrDefault(a => a.Id == id);
        if (existente is null)
            return NotFound();

        return View(new AerolineaViewModel
        {
            Id = existente.Id,
            Codigo = existente.Codigo,
            Nombre = existente.Nombre
        });
    }

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aerolinea(AerolineaViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var resultado = await _catalogo.GuardarAerolineaAsync(modelo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Exito"] = modelo.EsEdicion
            ? $"Aerolínea {modelo.Codigo} actualizada."
            : $"Aerolínea {modelo.Codigo} registrada.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesactivarAerolinea(Guid id)
    {
        var resultado = await _catalogo.DesactivarAerolineaAsync(id);
        MostrarResultado(resultado, "Aerolínea desactivada.");
        return RedirectToAction(nameof(Index));
    }

    // ---------- Aeropuertos ----------

    [HttpGet]
    [Authorize(Roles = RolAdmin)]
    public async Task<IActionResult> Aeropuerto(Guid? id)
    {
        if (id is null)
            return View(new AeropuertoViewModel());

        var existente = (await _catalogo.ObtenerAeropuertosAsync()).FirstOrDefault(a => a.Id == id);
        if (existente is null)
            return NotFound();

        return View(new AeropuertoViewModel
        {
            Id = existente.Id,
            Codigo = existente.Codigo,
            Nombre = existente.Nombre,
            Pais = existente.Pais
        });
    }

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Aeropuerto(AeropuertoViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(modelo);

        var resultado = await _catalogo.GuardarAeropuertoAsync(modelo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(modelo);
        }

        TempData["Exito"] = modelo.EsEdicion
            ? $"Aeropuerto {modelo.Codigo} actualizado."
            : $"Aeropuerto {modelo.Codigo} registrado.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesactivarAeropuerto(Guid id)
    {
        var resultado = await _catalogo.DesactivarAeropuertoAsync(id);
        MostrarResultado(resultado, "Aeropuerto desactivado.");
        return RedirectToAction(nameof(Index));
    }

    private void MostrarResultado(ResultadoOperacion resultado, string mensajeExito)
    {
        if (resultado.Exito)
            TempData["Exito"] = mensajeExito;
        else
            TempData["Error"] = resultado.Error;
    }
}
