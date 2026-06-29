using Microsoft.Extensions.DependencyInjection;

namespace SIV.Modules.Catalogo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogoModule(this IServiceCollection services)
    {
        services.AddScoped<ICatalogoService, CatalogoService>();
        return services;
    }
}
