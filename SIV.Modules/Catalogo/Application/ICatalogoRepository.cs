using SIV.Modules.Catalogo.Domain;

namespace SIV.Modules.Catalogo.Application;

public interface ICatalogoRepository
{
    Task<IReadOnlyList<Aerolinea>> ObtenerAerolineasAsync();
    Task<Aerolinea?> ObtenerAerolineaPorIdAsync(Guid id);
    Task<Aerolinea?> ObtenerAerolineaPorCodigoAsync(string codigo);
    Task GuardarAerolineaAsync(Aerolinea aerolinea);
    Task EliminarAerolineaAsync(Guid id);

    Task<IReadOnlyList<Aeropuerto>> ObtenerAeropuertosAsync();
    Task<Aeropuerto?> ObtenerAeropuertoPorIdAsync(Guid id);
    Task<Aeropuerto?> ObtenerAeropuertoPorCodigoAsync(string codigo);
    Task GuardarAeropuertoAsync(Aeropuerto aeropuerto);
    Task EliminarAeropuertoAsync(Guid id);
}
