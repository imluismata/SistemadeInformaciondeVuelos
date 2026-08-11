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
        if (vuelo is null)
            return NotFound();

        // Opciones para el desplegable de "cambio de puerta" del formulario de
        // cambio operativo. Se excluye la opción "sin asignar": un cambio de
        // puerta siempre apunta a una puerta concreta.
        ViewBag.Puertas = await PuertasOpcionesAsync(vuelo.PuertaId, incluirSinAsignar: false);
        return View(vuelo);
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
            PuertaId = vuelo.PuertaId,
            Aerolineas = aerolineas,
            Aeropuertos = aeropuertos,
            Puertas = await PuertasOpcionesAsync(vuelo.PuertaId)
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
            modelo.Puertas = await PuertasOpcionesAsync(modelo.PuertaId);
            return View(modelo);
        }

        var resultado = await _vuelos.ActualizarAsync(modelo);
        if (!resultado.Exito)
        {
            ModelState.AddModelError(string.Empty, resultado.Error!);
            (modelo.Aerolineas, modelo.Aeropuertos) = await CatalogoActivoAsync();
            modelo.Puertas = await PuertasOpcionesAsync(modelo.PuertaId);
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

    // ---------- Importación masiva (CSV/Excel) ----------

    [HttpGet]
    [Authorize(Roles = RolesOperacion)]
    public async Task<IActionResult> Importar()
    {
        // El catálogo activo alimenta los desplegables del editor por fila (modal). Como la
        // importación trabaja por código, los <option> usan el código como valor.
        var aerolineas = (await _catalogo.ObtenerAerolineasAsync())
            .Where(a => a.Activa)
            .Select(a => new { codigo = a.Codigo, etiqueta = $"{a.Codigo} — {a.Nombre}" });

        var aeropuertos = (await _catalogo.ObtenerAeropuertosAsync())
            .Where(a => a.Activo)
            .Select(a => new { codigo = a.Codigo, etiqueta = $"{a.Codigo} — {a.Nombre}" });

        var puertas = (await _catalogo.ObtenerPuertasAsync())
            .Where(p => p.Activa)
            .OrderBy(p => p.TerminalNombre ?? "￿").ThenBy(p => p.Codigo)
            .Select(p => new
            {
                codigo = p.Codigo,
                etiqueta = p.EsRampa ? $"{p.Codigo} (rampa)"
                         : p.TerminalNombre is { } t ? $"{p.Codigo} · {t}" : p.Codigo
            });

        ViewBag.Aerolineas = System.Text.Json.JsonSerializer.Serialize(aerolineas);
        ViewBag.Aeropuertos = System.Text.Json.JsonSerializer.Serialize(aeropuertos);
        ViewBag.Puertas = System.Text.Json.JsonSerializer.Serialize(puertas);
        return View();
    }

    // Endpoints AJAX de la página de importación. 'Previsualizar' sube el archivo (multipart)
    // y la API lo lee una vez; a partir de ahí se trabaja sobre las filas editables, que
    // 'Validar' e 'Importar' reciben como JSON. Todos devuelven el resultado como JSON.
    [HttpPost]
    [Authorize(Roles = RolesOperacion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> PrevisualizarImportacion(IFormFile? archivo)
    {
        if (archivo is null || archivo.Length == 0)
            return Json(new ImportacionRespuesta(false, "Adjunta un archivo .csv o .xlsx.", null));

        await using var flujo = archivo.OpenReadStream();
        return Json(await _vuelos.PrevisualizarImportacionAsync(flujo, archivo.FileName));
    }

    [HttpPost]
    [Authorize(Roles = RolesOperacion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ValidarImportacion([FromBody] List<FilaVueloApi> filas)
        => Json(await _vuelos.ValidarFilasAsync(filas ?? []));

    [HttpPost]
    [Authorize(Roles = RolesOperacion)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ImportarFilas([FromBody] List<FilaVueloApi> filas)
        => Json(await _vuelos.ImportarFilasAsync(filas ?? []));

    // Plantilla de ejemplo para que el usuario sepa el formato exacto de las columnas.
    [HttpGet]
    [Authorize(Roles = RolesOperacion)]
    public IActionResult PlantillaImportacion()
    {
        const string csv = "numero,aerolinea,origen,destino,salida,llegada,puerta\n" +
                           "QF 720,QF,SDQ,MIA,2026-09-01 10:00,2026-09-01 13:00,B5\n" +
                           "AA 949,AA,JFK,SDQ,2026-09-01 14:00,2026-09-01 18:30,\n";
        return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "plantilla-vuelos.csv");
    }

    /// <summary>
    /// Rellena las listas desplegables del formulario de registro con el catálogo vigente.
    /// </summary>
    private async Task<RegistrarVueloViewModel> ConCatalogoAsync(RegistrarVueloViewModel modelo)
    {
        (modelo.Aerolineas, modelo.Aeropuertos) = await CatalogoActivoAsync();
        modelo.Puertas = await PuertasOpcionesAsync(modelo.PuertaId);
        return modelo;
    }

    /// <summary>
    /// Opciones del desplegable de puertas, agrupadas por terminal (las rampas
    /// abiertas van en su propio grupo). Solo puertas activas, más la
    /// actualmente seleccionada aunque esté inactiva, para no perderla al editar.
    /// </summary>
    private async Task<IEnumerable<SelectListItem>> PuertasOpcionesAsync(Guid? seleccionada, bool incluirSinAsignar = true)
    {
        var puertas = await _catalogo.ObtenerPuertasAsync();

        var opciones = new List<SelectListItem>();
        if (incluirSinAsignar)
            opciones.Add(new SelectListItem("— Por asignar —", string.Empty, seleccionada is null));

        // Se listan las activas y, si se edita, también la ya asignada aunque esté inactiva.
        var visibles = puertas.Where(p => p.Activa || p.Id == seleccionada)
                              .OrderBy(p => p.TerminalNombre ?? "￿") // rampas al final
                              .ThenBy(p => p.Codigo);

        // El <optgroup> agrupa por instancia de SelectListGroup, así que se reutiliza
        // una sola por nombre de terminal.
        var grupos = new Dictionary<string, SelectListGroup>();

        foreach (var p in visibles)
        {
            var nombreGrupo = p.TerminalNombre ?? "Rampa abierta";
            if (!grupos.TryGetValue(nombreGrupo, out var grupo))
                grupos[nombreGrupo] = grupo = new SelectListGroup { Name = nombreGrupo };

            opciones.Add(new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.EsRampa ? $"{p.Codigo} (rampa)" : p.Codigo,
                Selected = p.Id == seleccionada,
                Group = grupo
            });
        }

        return opciones;
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
