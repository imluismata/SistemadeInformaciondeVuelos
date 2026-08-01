using SIV.Shared.DTOs;

namespace SIV.Shared.Contracts;

/// <summary>
/// Contrato de solo lectura que expone el módulo de Vuelos para que otros módulos
/// consulten el estado de los vuelos sin acceder a sus clases internas (DA-02).
///
/// Lo consume el módulo de Catálogo para la regla del SAD (CU-CAT-04): no se puede
/// desactivar una aerolínea/aeropuerto con vuelos activos. Un vuelo está activo
/// mientras no esté en un estado final (Completado o Cancelado). También lo consume
/// el módulo de Reportes para leer los vuelos de un período sin acoplarse a Vuelos.
/// </summary>
public interface IVueloConsulta
{
    Task<bool> ExistenVuelosActivosParaAerolineaAsync(Guid aerolineaId);
    Task<bool> ExistenVuelosActivosParaAeropuertoAsync(Guid aeropuertoId);

    // Vuelos cuyo horario de salida cae en el período [desde, hasta] (ambos
    // opcionales). Incluye historial de estados y cambios operativos para que
    // Reportes pueda agregarlos.
    Task<IReadOnlyList<VueloDto>> ObtenerParaReporteAsync(DateTime? desde, DateTime? hasta);
}
