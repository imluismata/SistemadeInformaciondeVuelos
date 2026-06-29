using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SIV.Modules.Auditoria.Application;
using SIV.Modules.Catalogo.Application;
using SIV.Modules.Vuelos.Application;
using SIV.Infrastructure.Repositories;

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

        return services;
    }
}
