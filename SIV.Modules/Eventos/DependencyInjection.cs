using Microsoft.Extensions.DependencyInjection;
using SIV.Shared.Events;

namespace SIV.Modules.Eventos;

public static class DependencyInjection
{
    /// <summary>
    /// Registra el publicador de eventos de dominio.
    /// Los manejadores (consumidores) los registra cada módulo interesado
    /// implementando IManejadorVueloCambiado; el publicador los recibe por inyección.
    /// </summary>
    public static IServiceCollection AddEventos(this IServiceCollection services)
    {
        services.AddScoped<IPublicadorEventos, PublicadorEventos>();
        return services;
    }
}
