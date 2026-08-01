namespace SIV.Shared.Contracts;

/// <summary>
/// Contrato de solo lectura que expone el módulo de Catálogo para que otros módulos
/// verifiquen la existencia de aerolíneas y aeropuertos sin acceder a sus clases
/// internas (DA-02).
///
/// Lo consume el módulo de Vuelos para hacer cumplir la regla del SAD: la aerolínea,
/// el origen y el destino deben existir previamente en el Catálogo Aeroportuario
/// antes de registrar o editar un vuelo.
/// </summary>
public interface ICatalogoConsulta
{
    Task<bool> ExisteAerolineaAsync(Guid aerolineaId);
    Task<bool> ExisteAeropuertoAsync(Guid aeropuertoId);
}
