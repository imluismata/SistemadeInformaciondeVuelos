using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIV.Infrastructure.Repositories;
using SIV.Modules.ConsultaPublica.Application;
using SIV.Modules.Notificaciones.Application;
using SIV.Modules.Seguimiento.Application;
using SIV.Modules.Usuarios.Application;

namespace SIV.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SivDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<ISeguimientoRepository, SeguimientoRepository>();
        services.AddScoped<INotificacionRepository, NotificacionRepository>();
        services.AddScoped<IConsultaPublicaRepository, ConsultaPublicaRepository>();

        return services;
    }
}
