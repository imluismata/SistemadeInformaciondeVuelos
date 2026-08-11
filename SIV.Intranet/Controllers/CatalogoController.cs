using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
            Aeropuertos = await _catalogo.ObtenerAeropuertosAsync(),
            Terminales = await _catalogo.ObtenerTerminalesAsync(),
            Puertas = await _catalogo.ObtenerPuertasAsync()
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

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReactivarAerolinea(Guid id)
    {
        var resultado = await _catalogo.ReactivarAerolineaAsync(id);
        MostrarResultado(resultado, "Aerolínea reactivada.");
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

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReactivarAeropuerto(Guid id)
    {
        var resultado = await _catalogo.ReactivarAeropuertoAsync(id);
        MostrarResultado(resultado, "Aeropuerto reactivado.");
        return RedirectToAction(nameof(Index));
    }

    // ---------- Terminales ----------

    [HttpGet]
    [Authorize(Roles = RolAdmin)]
    public async Task<IActionResult> Terminal(Guid? id)
    {
        var aeropuertos = await OpcionesAeropuertosAsync();

        if (id is null)
            return View(new TerminalViewModel { Aeropuertos = aeropuertos });

        var existente = (await _catalogo.ObtenerTerminalesAsync()).FirstOrDefault(t => t.Id == id);
        if (existente is null)
            return NotFound();

        return View(new TerminalViewModel
        {
            Id = existente.Id,
            Codigo = existente.Codigo,
            Nombre = existente.Nombre,
            AeropuertoId = existente.AeropuertoId,
            Aeropuertos = aeropuertos
        });
    }

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Terminal(TerminalViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            modelo.Aeropuertos = await OpcionesAeropuertosAsync();
            return View(modelo);
        }

        var resultado = await _catalogo.GuardarTerminalAsync(modelo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            modelo.Aeropuertos = await OpcionesAeropuertosAsync();
            return View(modelo);
        }

        TempData["Exito"] = modelo.EsEdicion
            ? $"Terminal {modelo.Codigo} actualizada."
            : $"Terminal {modelo.Codigo} registrada.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesactivarTerminal(Guid id)
    {
        MostrarResultado(await _catalogo.DesactivarTerminalAsync(id), "Terminal desactivada.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReactivarTerminal(Guid id)
    {
        MostrarResultado(await _catalogo.ReactivarTerminalAsync(id), "Terminal reactivada.");
        return RedirectToAction(nameof(Index));
    }

    // ---------- Puertas ----------

    [HttpGet]
    [Authorize(Roles = RolAdmin)]
    public async Task<IActionResult> Puerta(Guid? id)
    {
        var terminales = await OpcionesTerminalesAsync();

        if (id is null)
            return View(new PuertaViewModel { Terminales = terminales });

        var existente = (await _catalogo.ObtenerPuertasAsync()).FirstOrDefault(p => p.Id == id);
        if (existente is null)
            return NotFound();

        return View(new PuertaViewModel
        {
            Id = existente.Id,
            Codigo = existente.Codigo,
            TerminalId = existente.TerminalId,
            Terminales = terminales
        });
    }

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Puerta(PuertaViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            modelo.Terminales = await OpcionesTerminalesAsync();
            return View(modelo);
        }

        var resultado = await _catalogo.GuardarPuertaAsync(modelo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            modelo.Terminales = await OpcionesTerminalesAsync();
            return View(modelo);
        }

        TempData["Exito"] = modelo.EsEdicion
            ? $"Puerta {modelo.Codigo} actualizada."
            : $"Puerta {modelo.Codigo} registrada.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DesactivarPuerta(Guid id)
    {
        MostrarResultado(await _catalogo.DesactivarPuertaAsync(id), "Puerta desactivada.");
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = RolAdmin)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ReactivarPuerta(Guid id)
    {
        MostrarResultado(await _catalogo.ReactivarPuertaAsync(id), "Puerta reactivada.");
        return RedirectToAction(nameof(Index));
    }

    // Aeropuertos activos para el desplegable del formulario de terminal.
    private async Task<IEnumerable<SelectListItem>> OpcionesAeropuertosAsync()
        => (await _catalogo.ObtenerAeropuertosAsync())
            .Where(a => a.Activo)
            .Select(a => new SelectListItem($"{a.Codigo} — {a.Nombre}", a.Id.ToString()));

    // Terminales activas para el desplegable del formulario de puerta.
    private async Task<IEnumerable<SelectListItem>> OpcionesTerminalesAsync()
        => (await _catalogo.ObtenerTerminalesAsync())
            .Where(t => t.Activa)
            .Select(t => new SelectListItem(t.Nombre, t.Id.ToString()));

    private void MostrarResultado(ResultadoOperacion resultado, string mensajeExito)
    {
        if (resultado.Exito)
            TempData["Exito"] = mensajeExito;
        else
            TempData["Error"] = resultado.Error;
    }
}
