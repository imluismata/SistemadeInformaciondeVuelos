namespace SIV.Modules.Vuelos.Domain;

internal sealed record HistorialEstado(
    Guid Id,
    Guid VueloId,
    EstadoVuelo EstadoAnterior,
    EstadoVuelo EstadoNuevo,
    DateTime OcurridoEn);
