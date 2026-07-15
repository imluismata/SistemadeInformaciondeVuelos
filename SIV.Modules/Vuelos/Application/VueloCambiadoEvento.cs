using SIV.Shared.Enums;
using SIV.Shared.Events;

namespace SIV.Modules.Vuelos.Application;

/// <summary>
/// Implementación concreta del evento de dominio que publica el módulo de Vuelos.
/// Es internal: los demás módulos la consumen únicamente a través del contrato
/// IVueloCambiadoEvento definido en SIV.Shared (DA-02).
/// </summary>
internal sealed record VueloCambiadoEvento(
    Guid VueloId,
    string NumeroVuelo,
    string EstadoAnterior,
    string EstadoNuevo,
    TipoCambio TipoCambio,
    string Causa,
    DateTime OcurridoEn) : IVueloCambiadoEvento;
