using Microsoft.Extensions.DependencyInjection;
using SIV.Modules.ConsultaPublica.Application;
using SIV.Modules.Notificaciones.Application;
using SIV.Modules.Seguimiento.Application;
using SIV.Modules.Usuarios.Application;
using SIV.Shared.Contracts;

namespace SIV.Modules;

public static class DependencyInjection
{
    public static IServiceCollection AddModules(this IServiceCollection services)
    {
        services.AddScoped<IUsuarioService, UsuarioService>();

        // SeguimientoService implementa ISeguimientoService e ISeguimientoConsulta
        // lo registro una sola vez y lo reutilizo para las dos interfaces
        services.AddScoped<SeguimientoService>();
        services.AddScoped<ISeguimientoService>(sp => sp.GetRequiredService<SeguimientoService>());
        services.AddScoped<ISeguimientoConsulta>(sp => sp.GetRequiredService<SeguimientoService>());

        services.AddScoped<INotificacionService, NotificacionService>();
        services.AddScoped<IConsultaPublicaService, ConsultaPublicaService>();

        return services;
    }
}
