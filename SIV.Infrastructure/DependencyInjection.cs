using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIV.Infrastructure.Configuracion;
using SIV.Infrastructure.Correo;
using SIV.Infrastructure.Repositories;
using SIV.Modules.Usuarios.Application.Interfaces;
using SIV.Modules.Auditoria.Application;
using SIV.Modules.Catalogo.Application;
using SIV.Modules.ConsultaPublica.Application;
using SIV.Modules.Notificaciones.Application;
using SIV.Modules.Seguimiento.Application;
using SIV.Modules.Usuarios.Application;
using SIV.Modules.Vuelos.Application;
using SIV.Shared.Contracts;

namespace SIV.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SivDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Unidad de trabajo: transacción atómica compartida por los módulos (DA-04).
        // Scoped para vivir en el mismo scope que el SivDbContext.
        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddScoped<IVueloRepository, VueloRepository>();
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        // Lector de archivos de importación de vuelos (CSV/Excel), puerto del módulo Vuelos.
        services.AddScoped<Modules.Vuelos.Application.ILectorVuelosImportados, Importacion.LectorVuelosImportados>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ISeguimientoRepository, SeguimientoRepository>();
        services.AddScoped<INotificacionRepository, NotificacionRepository>();
        services.AddScoped<IConsultaPublicaRepository, ConsultaPublicaRepository>();

        // Envío de correos (verificación de cuenta). Se leen los valores a mano
        // para no depender de paquetes extra de binding de configuración.
        var seccionCorreo = configuration.GetSection(OpcionesCorreo.Seccion);
        var opcionesCorreo = new OpcionesCorreo
        {
            Habilitado = bool.TryParse(seccionCorreo["Habilitado"], out var habilitado) && habilitado,
            Host = seccionCorreo["Host"] ?? "smtp.gmail.com",
            Puerto = int.TryParse(seccionCorreo["Puerto"], out var puerto) ? puerto : 587,
            Usuario = seccionCorreo["Usuario"] ?? string.Empty,
            Clave = seccionCorreo["Clave"] ?? string.Empty,
            RemitenteNombre = seccionCorreo["RemitenteNombre"] ?? "Quisqueya Flight Hub",
        };
        services.AddSingleton(opcionesCorreo);
        // ServicioCorreoSmtp implementa dos contratos: IServicioCorreo (Usuarios) e
        // INotificadorPorCorreo (notificaciones de vuelo). Se comparte por scope.
        services.AddScoped<ServicioCorreoSmtp>();
        services.AddScoped<IServicioCorreo>(sp => sp.GetRequiredService<ServicioCorreoSmtp>());
        services.AddScoped<INotificadorPorCorreo>(sp => sp.GetRequiredService<ServicioCorreoSmtp>());

        // Notificaciones por correo asíncronas (RNF-REN-03): el manejador del evento
        // encola el correo (instantáneo) y el worker lo envía por SMTP en segundo
        // plano, fuera del request y de la transacción atómica del cambio de vuelo.
        // La cola es singleton (una sola compartida); el worker, un hosted service.
        services.AddSingleton<IColaCorreos, ColaCorreosEnMemoria>();
        services.AddHostedService<EnviadorCorreosHostedService>();

        // Aeropuerto base para los tableros de salidas y llegadas (AILA = SDQ).
        var opcionesAeropuerto = new OpcionesAeropuerto
        {
            Codigo = configuration.GetSection(OpcionesAeropuerto.Seccion)["Codigo"] ?? "SDQ",
        };
        services.AddSingleton(opcionesAeropuerto);

        return services;
    }
}
