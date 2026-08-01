using SIV.Shared.Contracts;

namespace SIV.Modules.Vuelos.Application;

/// <summary>
/// Implementación del contrato compartido <see cref="IVueloConsulta"/>. Es el punto
/// por el que otros módulos (Catálogo) preguntan por vuelos activos sin conocer las
/// entidades internas de Vuelos. Clase de solo lectura, separada de VueloService para
/// no mezclar consultas cross-módulo con la orquestación de escritura (SRP / ISP).
/// </summary>
internal sealed class VueloConsultaService(IVueloRepository repository) : IVueloConsulta
{
    public Task<bool> ExistenVuelosActivosParaAerolineaAsync(Guid aerolineaId)
        => repository.ExistenVuelosActivosPorAerolineaAsync(aerolineaId);

    public Task<bool> ExistenVuelosActivosParaAeropuertoAsync(Guid aeropuertoId)
        => repository.ExistenVuelosActivosPorAeropuertoAsync(aeropuertoId);
}
