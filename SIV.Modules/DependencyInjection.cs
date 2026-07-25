using Microsoft.Extensions.DependencyInjection;
using SIV.Modules.ConsultaPublica.Application.Interfaces;
using SIV.Modules.ConsultaPublica.Application.Services;
using SIV.Modules.Notificaciones.Application.Interfaces;
using SIV.Modules.Notificaciones.Application.Services;
using SIV.Modules.Seguimiento.Application.Interfaces;
using SIV.Modules.Seguimiento.Application.Services;
using SIV.Modules.Usuarios.Application.Interfaces;
using SIV.Modules.Usuarios.Application.Services;
using SIV.Shared.Contracts;
using SIV.Shared.Events;

namespace SIV.Modules;

public static class DependencyInjection
{
    public static IServiceCollection AddModules(this IServiceCollection services)
    {
        // Usuarios
        services.AddScoped<IUsuarioService, UsuarioService>();

        // Seguimiento — implementa ISeguimientoService e ISeguimientoConsulta
        services.AddScoped<SeguimientoService>();
        services.AddScoped<ISeguimientoService>(sp => sp.GetRequiredService<SeguimientoService>());
        services.AddScoped<ISeguimientoConsulta>(sp => sp.GetRequiredService<SeguimientoService>());

        // Notificaciones
        services.AddScoped<INotificacionService, NotificacionService>();
        // Conecta el evento de cambio de vuelo con la generación de notificaciones.
        services.AddScoped<IManejadorVueloCambiado, ManejadorVueloCambiado>();

        // Consulta Pública
        services.AddScoped<IConsultaPublicaService, ConsultaPublicaService>();

        return services;
    }
}
