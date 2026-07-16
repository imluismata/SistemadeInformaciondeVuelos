using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIV.Infrastructure.Repositories;
using SIV.Modules.Auditoria.Application;
using SIV.Modules.Catalogo.Application;
using SIV.Modules.ConsultaPublica.Application;
using SIV.Modules.Notificaciones.Application;
using SIV.Modules.Seguimiento.Application;
using SIV.Modules.Usuarios.Application;
using SIV.Modules.Vuelos.Application;

namespace SIV.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<SivDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IVueloRepository, VueloRepository>();
        services.AddScoped<ICatalogoRepository, CatalogoRepository>();
        services.AddScoped<IAuditoriaRepository, AuditoriaRepository>();

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ISeguimientoRepository, SeguimientoRepository>();
        services.AddScoped<INotificacionRepository, NotificacionRepository>();
        services.AddScoped<IConsultaPublicaRepository, ConsultaPublicaRepository>();

        return services;
    }
}
