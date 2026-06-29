using Microsoft.Extensions.DependencyInjection;
using SIV.Modules.Vuelos.Domain;

namespace SIV.Modules.Vuelos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVuelosModule(this IServiceCollection services)
    {
        services.AddScoped<IVueloDomainService, VueloDomainService>();
        services.AddScoped<IVueloService, VueloService>();
        return services;
    }
}
