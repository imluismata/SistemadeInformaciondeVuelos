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

    /// <summary>
    /// Id del aeropuerto base del sistema (AILA/SDQ, según configuración), o null si aún
    /// no existe en el catálogo. Lo usa el módulo de Vuelos para saber si un vuelo sale de
    /// o llega a la base al validar la ocupación de sus puertas, sin conocer la config ni
    /// las entidades internas de Catálogo (DA-02).
    /// </summary>
    Task<Guid?> ObtenerAeropuertoBaseIdAsync();

    // Puertas: usadas por el módulo de Vuelos para validar la puerta asignada y
    // resolver su descripción (código + terminal) al mostrarla, sin conocer las
    // entidades internas de Catálogo (DA-02).
    Task<bool> ExistePuertaAsync(Guid puertaId);
    Task<PuertaResumen?> ObtenerPuertaAsync(Guid puertaId);

    // Resolución por código (IATA / código de aerolínea / código de puerta). La usa la
    // importación masiva de vuelos, donde el archivo trae códigos legibles ("QF", "SDQ",
    // "B5") en vez de identificadores. Devuelven null si el código no existe.
    Task<Guid?> ObtenerAerolineaIdPorCodigoAsync(string codigo);
    Task<Guid?> ObtenerAeropuertoIdPorCodigoAsync(string codigo);
    Task<PuertaResumen?> ObtenerPuertaPorCodigoAsync(string codigo);
}

/// <summary>Resumen de una puerta para mostrarla en otros módulos. <see cref="TerminalNombre"/>
/// es nulo si es una rampa abierta.</summary>
public sealed record PuertaResumen(Guid Id, string Codigo, string? TerminalNombre, bool EsRampa);
