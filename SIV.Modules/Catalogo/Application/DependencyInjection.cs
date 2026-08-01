using Microsoft.Extensions.DependencyInjection;
using SIV.Shared.Contracts;

namespace SIV.Modules.Catalogo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogoModule(this IServiceCollection services)
    {
        services.AddScoped<ICatalogoService, CatalogoService>();
        // Contrato de consulta que otros módulos (Vuelos) usan vía SIV.Shared.
        services.AddScoped<ICatalogoConsulta, CatalogoConsultaService>();
        return services;
    }
}
