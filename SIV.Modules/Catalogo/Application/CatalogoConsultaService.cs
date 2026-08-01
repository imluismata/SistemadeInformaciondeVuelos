using SIV.Shared.Contracts;

namespace SIV.Modules.Catalogo.Application;

/// <summary>
/// Implementación del contrato compartido <see cref="ICatalogoConsulta"/>. Es el
/// punto por el que otros módulos (Vuelos) verifican que una aerolínea o aeropuerto
/// existe, sin conocer las entidades internas de Catálogo. Solo lectura, separada de
/// CatalogoService para no mezclar consulta cross-módulo con la escritura (SRP).
/// </summary>
internal sealed class CatalogoConsultaService(ICatalogoRepository repository) : ICatalogoConsulta
{
    public async Task<bool> ExisteAerolineaAsync(Guid aerolineaId)
        => await repository.ObtenerAerolineaPorIdAsync(aerolineaId) is not null;

    public async Task<bool> ExisteAeropuertoAsync(Guid aeropuertoId)
        => await repository.ObtenerAeropuertoPorIdAsync(aeropuertoId) is not null;
}
