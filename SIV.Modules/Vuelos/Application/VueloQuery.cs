using SIV.Modules.Vuelos.Domain;

namespace SIV.Modules.Vuelos.Application;

public sealed record ConsultarVuelosQuery(
    Guid? AerolineaId = null,
    Guid? AeropuertoOrigenId = null,
    Guid? AeropuertoDestinoId = null,
    DateTime? FechaDesde = null,
    DateTime? FechaHasta = null,
    EstadoVuelo? Estado = null);
