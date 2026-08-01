using Microsoft.Extensions.DependencyInjection;

namespace SIV.Modules.Reportes.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddReportesModule(this IServiceCollection services)
    {
        services.AddScoped<IReporteService, ReporteService>();
        return services;
    }
}
