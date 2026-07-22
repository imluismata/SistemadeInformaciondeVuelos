using System.Net;
using System.Net.Http.Json;
using SIV.Intranet.Models;

namespace SIV.Intranet.Services;

/// <summary>
/// Implementación del cliente de la API basada en HttpClient tipado.
/// No contiene lógica de negocio: solo traduce llamadas a peticiones HTTP
/// y deserializa las respuestas.
/// </summary>
public sealed class SivApiClient : ISivApiClient
{
    private readonly HttpClient _http;

    public SivApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<ResultadoLogin?> LoginAsync(string email, string password)
    {
        var respuesta = await _http.PostAsJsonAsync("api/auth/login", new { email, password });

        if (respuesta.StatusCode == HttpStatusCode.Unauthorized)
            return null;

        respuesta.EnsureSuccessStatusCode();
        return await respuesta.Content.ReadFromJsonAsync<ResultadoLogin>();
    }

    public async Task<IReadOnlyList<VueloApi>> ObtenerVuelosAsync()
    {
        var vuelos = await _http.GetFromJsonAsync<List<VueloApi>>("api/vuelos");
        return vuelos ?? [];
    }
}
