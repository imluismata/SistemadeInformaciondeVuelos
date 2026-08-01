using SIV.Shared.Contracts;
using SIV.Shared.DTOs;

namespace SIV.Modules.Vuelos.Application;

/// <summary>
/// Implementación del contrato compartido <see cref="IVueloConsulta"/>. Es el punto
/// por el que otros módulos (Catálogo, Reportes) consultan los vuelos sin conocer las
/// entidades internas de Vuelos. Clase de solo lectura, separada de VueloService para
/// no mezclar consultas cross-módulo con la orquestación de escritura (SRP / ISP).
/// </summary>
internal sealed class VueloConsultaService(IVueloRepository repository) : IVueloConsulta
{
    public Task<bool> ExistenVuelosActivosParaAerolineaAsync(Guid aerolineaId)
        => repository.ExistenVuelosActivosPorAerolineaAsync(aerolineaId);

    public Task<bool> ExistenVuelosActivosParaAeropuertoAsync(Guid aeropuertoId)
        => repository.ExistenVuelosActivosPorAeropuertoAsync(aeropuertoId);

    public async Task<IReadOnlyList<VueloDto>> ObtenerParaReporteAsync(DateTime? desde, DateTime? hasta)
    {
        // Reutiliza la consulta con filtro de fechas del repositorio (por horario
        // de salida) y mapea al DTO compartido con historial y cambios operativos.
        var vuelos = await repository.ConsultarAsync(new ConsultarVuelosQuery(FechaDesde: desde, FechaHasta: hasta));
        return vuelos.Select(VueloMapper.ADto).ToList();
    }
}
