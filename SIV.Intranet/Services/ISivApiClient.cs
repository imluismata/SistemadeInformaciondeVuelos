using SIV.Intranet.Models;

namespace SIV.Intranet.Services;

/// <summary>
/// Cliente de la API del SIV. Es la única pieza de la intranet que conoce
/// los endpoints HTTP; los controladores delegan en ella (igual que los
/// controladores de la API delegan en los Services).
/// </summary>
public interface ISivApiClient
{
    Task<ResultadoLogin?> LoginAsync(string email, string password);
    Task<IReadOnlyList<VueloApi>> ObtenerVuelosAsync();
}
