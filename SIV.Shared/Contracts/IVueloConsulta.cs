namespace SIV.Shared.Contracts;

/// <summary>
/// Contrato de solo lectura que expone el módulo de Vuelos para que otros módulos
/// consulten el estado de los vuelos sin acceder a sus clases internas (DA-02).
///
/// Lo consume el módulo de Catálogo para hacer cumplir la regla del SAD (CU-CAT-04):
/// no se puede desactivar una aerolínea o aeropuerto que aún tenga vuelos activos.
/// Un vuelo se considera activo mientras no esté en un estado final (Completado o
/// Cancelado).
/// </summary>
public interface IVueloConsulta
{
    Task<bool> ExistenVuelosActivosParaAerolineaAsync(Guid aerolineaId);
    Task<bool> ExistenVuelosActivosParaAeropuertoAsync(Guid aeropuertoId);
}
