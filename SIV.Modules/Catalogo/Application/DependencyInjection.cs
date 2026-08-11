using Microsoft.Extensions.DependencyInjection;
using SIV.Shared.Contracts;

namespace SIV.Modules.Catalogo.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddCatalogoModule(this IServiceCollection services, string codigoAeropuertoBase)
    {
        services.AddScoped<ICatalogoService, CatalogoService>();
        // Contrato de consulta que otros módulos (Vuelos) usan vía SIV.Shared. Se le pasa el
        // código del aeropuerto base para poder resolver su Id sin conocer la configuración.
        services.AddScoped<ICatalogoConsulta>(sp =>
            new CatalogoConsultaService(sp.GetRequiredService<ICatalogoRepository>(), codigoAeropuertoBase));
        return services;
    }
}
