namespace SIV.Modules.Vuelos.Domain;

public sealed record HistorialEstado(
    Guid Id,
    Guid VueloId,
    EstadoVuelo EstadoAnterior,
    EstadoVuelo EstadoNuevo,
    DateTime OcurridoEn);
