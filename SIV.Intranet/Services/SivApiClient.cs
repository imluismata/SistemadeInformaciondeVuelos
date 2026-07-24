using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using SIV.Intranet.Models;

namespace SIV.Intranet.Services;

/// <summary>
/// Traduce la respuesta HTTP de la API a un ResultadoOperacion, extrayendo
/// el mensaje de negocio que devuelve el middleware de errores del backend.
/// Centralizado aquí para no repetirlo en cada llamada (DRY).
/// </summary>
internal static class RespuestaApi
{
    public static async Task<ResultadoOperacion> InterpretarAsync(HttpResponseMessage respuesta)
    {
        if (respuesta.IsSuccessStatusCode)
            return ResultadoOperacion.Ok();

        if (respuesta.StatusCode == HttpStatusCode.Forbidden)
            return ResultadoOperacion.Fallo("Tu rol no tiene permiso para realizar esta acción.");

        var cuerpo = await respuesta.Content.ReadAsStringAsync();

        // El ExceptionMiddleware de la API responde { "error": "mensaje" }.
        try
        {
            using var json = JsonDocument.Parse(cuerpo);
            if (json.RootElement.TryGetProperty("error", out var mensaje))
                return ResultadoOperacion.Fallo(mensaje.GetString() ?? "Error desconocido.");
        }
        catch (JsonException)
        {
            // La respuesta no era JSON; se usa el cuerpo tal cual.
        }

        return ResultadoOperacion.Fallo(string.IsNullOrWhiteSpace(cuerpo)
            ? $"La operación falló ({(int)respuesta.StatusCode})."
            : cuerpo);
    }
}

public sealed class AutenticacionApi : IAutenticacionApi
{
    private readonly HttpClient _http;

    public AutenticacionApi(HttpClient http) => _http = http;

    public async Task<ResultadoLogin?> LoginAsync(string email, string password)
    {
        var respuesta = await _http.PostAsJsonAsync("api/auth/login", new { email, password });

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<ResultadoLogin>();
    }
}

public sealed class CatalogoApi : ICatalogoApi
{
    private readonly HttpClient _http;

    public CatalogoApi(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<AerolineaApi>> ObtenerAerolineasAsync()
        => await _http.GetFromJsonAsync<List<AerolineaApi>>("api/catalogo/aerolineas") ?? [];

    public async Task<IReadOnlyList<AeropuertoApi>> ObtenerAeropuertosAsync()
        => await _http.GetFromJsonAsync<List<AeropuertoApi>>("api/catalogo/aeropuertos") ?? [];

    public async Task<ResultadoOperacion> GuardarAerolineaAsync(AerolineaViewModel aerolinea)
    {
        var cuerpo = new { codigo = aerolinea.Codigo, nombre = aerolinea.Nombre };

        var respuesta = aerolinea.EsEdicion
            ? await _http.PutAsJsonAsync($"api/catalogo/aerolineas/{aerolinea.Id}", cuerpo)
            : await _http.PostAsJsonAsync("api/catalogo/aerolineas", cuerpo);

        return await RespuestaApi.InterpretarAsync(respuesta);
    }

    public async Task<ResultadoOperacion> DesactivarAerolineaAsync(Guid id)
        => await RespuestaApi.InterpretarAsync(await _http.DeleteAsync($"api/catalogo/aerolineas/{id}"));

    public async Task<ResultadoOperacion> GuardarAeropuertoAsync(AeropuertoViewModel aeropuerto)
    {
        var cuerpo = new { codigo = aeropuerto.Codigo, nombre = aeropuerto.Nombre, pais = aeropuerto.Pais };

        var respuesta = aeropuerto.EsEdicion
            ? await _http.PutAsJsonAsync($"api/catalogo/aeropuertos/{aeropuerto.Id}", cuerpo)
            : await _http.PostAsJsonAsync("api/catalogo/aeropuertos", cuerpo);

        return await RespuestaApi.InterpretarAsync(respuesta);
    }

    public async Task<ResultadoOperacion> DesactivarAeropuertoAsync(Guid id)
        => await RespuestaApi.InterpretarAsync(await _http.DeleteAsync($"api/catalogo/aeropuertos/{id}"));
}

public sealed class AuditoriaApiClient : IAuditoriaApi
{
    private readonly HttpClient _http;

    public AuditoriaApiClient(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<AuditoriaApi>> ConsultarAsync(
        string? modulo, string? accion, DateTime? desde, DateTime? hasta)
    {
        var parametros = new List<string>();

        if (!string.IsNullOrWhiteSpace(modulo))
            parametros.Add($"modulo={Uri.EscapeDataString(modulo)}");

        if (!string.IsNullOrWhiteSpace(accion))
            parametros.Add($"accion={Uri.EscapeDataString(accion)}");

        if (desde.HasValue)
            parametros.Add($"desde={desde.Value:o}");

        // Se incluye el día completo de la fecha "hasta".
        if (hasta.HasValue)
            parametros.Add($"hasta={hasta.Value.Date.AddDays(1).AddTicks(-1):o}");

        var consulta = parametros.Count > 0 ? "?" + string.Join("&", parametros) : string.Empty;

        return await _http.GetFromJsonAsync<List<AuditoriaApi>>($"api/auditoria{consulta}") ?? [];
    }
}

public sealed class VuelosApi : IVuelosApi
{
    private readonly HttpClient _http;

    public VuelosApi(HttpClient http) => _http = http;

    public async Task<IReadOnlyList<VueloApi>> ObtenerTodosAsync()
        => await _http.GetFromJsonAsync<List<VueloApi>>("api/vuelos") ?? [];

    public async Task<VueloDetalleApi?> ObtenerPorIdAsync(Guid id)
    {
        var respuesta = await _http.GetAsync($"api/vuelos/{id}");
        if (respuesta.StatusCode == HttpStatusCode.NotFound)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<VueloDetalleApi>();
    }

    public async Task<ResultadoOperacion> RegistrarAsync(RegistrarVueloViewModel vuelo)
    {
        var respuesta = await _http.PostAsJsonAsync("api/vuelos", new
        {
            numero = vuelo.Numero,
            aerolineaId = vuelo.AerolineaId,
            aeropuertoOrigenId = vuelo.AeropuertoOrigenId,
            aeropuertoDestinoId = vuelo.AeropuertoDestinoId,
            horarioSalida = vuelo.HorarioSalida,
            horarioLlegada = vuelo.HorarioLlegada,
            puerta = vuelo.Puerta
        });

        return await RespuestaApi.InterpretarAsync(respuesta);
    }

    public async Task<ResultadoOperacion> CambiarEstadoAsync(Guid id, string estadoNuevo)
    {
        var respuesta = await _http.PutAsJsonAsync($"api/vuelos/{id}/estado", new { estadoNuevo });
        return await RespuestaApi.InterpretarAsync(respuesta);
    }

    public async Task<ResultadoOperacion> RegistrarCambioOperativoAsync(Guid id, CambioOperativoViewModel cambio)
    {
        var respuesta = await _http.PostAsJsonAsync($"api/vuelos/{id}/cambios-operativos", new
        {
            tipo = cambio.Tipo,
            motivo = cambio.Motivo,
            duracion = string.IsNullOrWhiteSpace(cambio.Duracion) ? null : cambio.Duracion,
            nuevaPuerta = string.IsNullOrWhiteSpace(cambio.NuevaPuerta) ? null : cambio.NuevaPuerta
        });

        return await RespuestaApi.InterpretarAsync(respuesta);
    }
}
