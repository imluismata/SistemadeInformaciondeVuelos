using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.Intranet.Services;

namespace SIV.Intranet.Controllers;

/// <summary>
/// Muestra los vuelos consumiendo la API. Requiere sesión iniciada;
/// no contiene lógica de negocio, solo pide los datos y los pasa a la vista.
/// </summary>
[Authorize]
public sealed class VuelosController : Controller
{
    private readonly ISivApiClient _api;

    public VuelosController(ISivApiClient api)
    {
        _api = api;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var vuelos = await _api.ObtenerVuelosAsync();
        return View(vuelos);
    }
}
