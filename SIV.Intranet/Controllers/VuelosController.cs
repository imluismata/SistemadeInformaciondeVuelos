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
    public async Task<IActionResult> Index(VuelosFiltroViewModel filtro)
    {
        // Si el usuario aplicó filtros, se consulta con ellos (CU-VUE-03); si no,
        // se traen todos los vuelos.
        filtro.Vuelos = filtro.HayFiltro
            ? await _vuelos.ConsultarAsync(filtro)
            : await _vuelos.ObtenerTodosAsync();

        var aerolineas = await _catalogo.ObtenerAerolineasAsync();
        filtro.Aerolineas = aerolineas.Select(a => new SelectListItem($"{a.Codigo} — {a.Nombre}", a.Id.ToString()));

        return View(filtro);
    }

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

    [HttpGet]
    [Authorize(Roles = RolesOperacion)]
    public async Task<IActionResult> Editar(Guid id)
    {
        var vuelo = await _vuelos.ObtenerPorIdAsync(id);
        if (vuelo is null)
            return NotFound();

        var (aerolineas, aeropuertos) = await CatalogoActivoAsync();
        var modelo = new EditarVueloViewModel
        {
            Id = vuelo.Id,
            Numero = vuelo.Numero,
            AerolineaId = vuelo.AerolineaId,
            AeropuertoOrigenId = vuelo.AeropuertoOrigenId,
            AeropuertoDestinoId = vuelo.AeropuertoDestinoId,
            HorarioSalida = vuelo.HorarioSalida,
            HorarioLlegada = vuelo.HorarioLlegada,
            Puerta = vuelo.Puerta,
            Aerolineas = aerolineas,
            Aeropuertos = aeropuertos
        };

        return View(modelo);
    }

    [HttpPost]
    [Authorize(Roles = RolesOperacion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(EditarVueloViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            (modelo.Aerolineas, modelo.Aeropuertos) = await CatalogoActivoAsync();
            return View(modelo);
        }

        var resultado = await _vuelos.ActualizarAsync(modelo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            (modelo.Aerolineas, modelo.Aeropuertos) = await CatalogoActivoAsync();
            return View(modelo);
        }

        TempData["Exito"] = $"Vuelo {modelo.Numero} actualizado.";
        return RedirectToAction(nameof(Detalle), new { id = modelo.Id });
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
    /// Rellena las listas desplegables del formulario de registro con el catálogo vigente.
    /// </summary>
    private async Task<RegistrarVueloViewModel> ConCatalogoAsync(RegistrarVueloViewModel modelo)
    {
        (modelo.Aerolineas, modelo.Aeropuertos) = await CatalogoActivoAsync();
        return modelo;
    }

    /// <summary>
    /// Opciones de aerolíneas y aeropuertos activos para los desplegables de los
    /// formularios de vuelo (registrar y editar), evitando duplicar el mapeo.
    /// </summary>
    private async Task<(IEnumerable<SelectListItem> aerolineas, IEnumerable<SelectListItem> aeropuertos)> CatalogoActivoAsync()
    {
        var aerolineas = await _catalogo.ObtenerAerolineasAsync();
        var aeropuertos = await _catalogo.ObtenerAeropuertosAsync();

        return (
            aerolineas.Where(a => a.Activa).Select(a => new SelectListItem($"{a.Codigo} — {a.Nombre}", a.Id.ToString())),
            aeropuertos.Where(a => a.Activo).Select(a => new SelectListItem($"{a.Codigo} — {a.Nombre}", a.Id.ToString()))
        );
    }
}
