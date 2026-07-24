using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SIV.Intranet.Models;
using SIV.Intranet.Services;

namespace SIV.Intranet.Controllers;

/// <summary>
/// Pantallas de vuelos. No contiene lógica de negocio: pide datos a la API,
/// los pasa a la vista y traslada los mensajes de error que devuelve el backend.
/// Las reglas (transiciones válidas, unicidad, etc.) viven en los módulos.
/// </summary>
[Authorize]
public sealed class VuelosController : Controller
{
    private const string RolesOperacion = "OperadorVuelos,Administrador";

    private readonly IVuelosApi _vuelos;
    private readonly ICatalogoApi _catalogo;

    public VuelosController(IVuelosApi vuelos, ICatalogoApi catalogo)
    {
        _vuelos = vuelos;
        _catalogo = catalogo;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
        => View(await _vuelos.ObtenerTodosAsync());

    [HttpGet]
    public async Task<IActionResult> Detalle(Guid id)
    {
        var vuelo = await _vuelos.ObtenerPorIdAsync(id);
        return vuelo is null ? NotFound() : View(vuelo);
    }

    [HttpGet]
    [Authorize(Roles = RolesOperacion)]
    public async Task<IActionResult> Registrar()
        => View(await ConCatalogoAsync(new RegistrarVueloViewModel()));

    [HttpPost]
    [Authorize(Roles = RolesOperacion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registrar(RegistrarVueloViewModel modelo)
    {
        if (!ModelState.IsValid)
            return View(await ConCatalogoAsync(modelo));

        var resultado = await _vuelos.RegistrarAsync(modelo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            return View(await ConCatalogoAsync(modelo));
        }

        TempData["Exito"] = $"Vuelo {modelo.Numero} registrado correctamente.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RolesOperacion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CambiarEstado(Guid id, string estadoNuevo)
    {
        var resultado = await _vuelos.CambiarEstadoAsync(id, estadoNuevo);

        if (resultado.Exito)
            TempData["Exito"] = $"Estado cambiado a {estadoNuevo}.";
        else
            TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Detalle), new { id });
    }

    [HttpPost]
    [Authorize(Roles = RolesOperacion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegistrarCambioOperativo(CambioOperativoViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Revisa los datos del cambio operativo: el motivo es obligatorio.";
            return RedirectToAction(nameof(Detalle), new { id = modelo.VueloId });
        }

        var resultado = await _vuelos.RegistrarCambioOperativoAsync(modelo.VueloId, modelo);

        if (resultado.Exito)
            TempData["Exito"] = $"Cambio operativo ({modelo.Tipo}) registrado.";
        else
            TempData["Error"] = resultado.Error;

        return RedirectToAction(nameof(Detalle), new { id = modelo.VueloId });
    }

    /// <summary>
    /// Rellena las listas desplegables del formulario con el catálogo vigente.
    /// </summary>
    private async Task<RegistrarVueloViewModel> ConCatalogoAsync(RegistrarVueloViewModel modelo)
    {
        var aerolineas = await _catalogo.ObtenerAerolineasAsync();
        var aeropuertos = await _catalogo.ObtenerAeropuertosAsync();

        modelo.Aerolineas = aerolineas
            .Where(a => a.Activa)
            .Select(a => new SelectListItem($"{a.Codigo} — {a.Nombre}", a.Id.ToString()));

        modelo.Aeropuertos = aeropuertos
            .Where(a => a.Activo)
            .Select(a => new SelectListItem($"{a.Codigo} — {a.Nombre}", a.Id.ToString()));

        return modelo;
    }
}
