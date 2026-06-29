namespace SIV.Shared.DTOs;

public sealed record HistorialEstadoDto(
    Guid Id,
    Guid VueloId,
    string EstadoAnterior,
    string EstadoNuevo,
    DateTime OcurridoEn);
