using Microsoft.Extensions.DependencyInjection;
using SIV.Modules.Vuelos.Domain;
using SIV.Shared.Contracts;

namespace SIV.Modules.Vuelos.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVuelosModule(this IServiceCollection services)
    {
        services.AddScoped<IVueloDomainService, VueloDomainService>();
        services.AddScoped<IVueloService, VueloService>();
        services.AddScoped<IVueloImportacionService, VueloImportacionService>();
        // Contrato de consulta que otros módulos (Catálogo) usan vía SIV.Shared.
        services.AddScoped<IVueloConsulta, VueloConsultaService>();
        return services;
    }
}
