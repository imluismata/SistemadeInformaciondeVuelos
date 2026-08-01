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
        // Usuarios — implementa IUsuarioService e IUsuarioConsulta (misma instancia por scope).
        services.AddScoped<UsuarioService>();
        services.AddScoped<IUsuarioService>(sp => sp.GetRequiredService<UsuarioService>());
        services.AddScoped<IUsuarioConsulta>(sp => sp.GetRequiredService<UsuarioService>());

        // Seguimiento — implementa ISeguimientoService e ISeguimientoConsulta
        services.AddScoped<SeguimientoService>();
        services.AddScoped<ISeguimientoService>(sp => sp.GetRequiredService<SeguimientoService>());
        services.AddScoped<ISeguimientoConsulta>(sp => sp.GetRequiredService<SeguimientoService>());

        // Notificaciones
        services.AddScoped<INotificacionService, NotificacionService>();
        // Consumidores del evento de cambio de vuelo. Se pueden agregar canales sin
        // modificar el publicador ni los manejadores existentes (Open/Closed):
        services.AddScoped<IManejadorVueloCambiado, ManejadorVueloCambiado>();       // in-app
        services.AddScoped<IManejadorVueloCambiado, ManejadorVueloCambiadoCorreo>(); // correo

        // Consulta Pública
        services.AddScoped<IConsultaPublicaService, ConsultaPublicaService>();

        return services;
    }
}
