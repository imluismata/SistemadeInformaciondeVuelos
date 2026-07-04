using Microsoft.Extensions.DependencyInjection;
using SIV.Shared.Contracts;

namespace SIV.Modules.Auditoria.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditoriaModule(this IServiceCollection services)
    {
        services.AddScoped<IAuditoriaService, AuditoriaService>();
        return services;
    }
}
