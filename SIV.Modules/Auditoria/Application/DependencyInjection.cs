using Microsoft.Extensions.DependencyInjection;

namespace SIV.Modules.Auditoria.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditoriaModule(this IServiceCollection services)
    {
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        return services;
    }
}
