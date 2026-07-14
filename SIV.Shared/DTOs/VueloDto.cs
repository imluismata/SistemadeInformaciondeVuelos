namespace SIV.Shared.DTOs;

public sealed record VueloDto(
    Guid Id,
    string Numero,
    Guid AerolineaId,
    Guid AeropuertoOrigenId,
    Guid AeropuertoDestinoId,
    DateTime HorarioSalida,
    DateTime HorarioLlegada,
    string? Puerta,
    string EstadoActual,
    IReadOnlyList<HistorialEstadoDto> HistorialEstados,
    IReadOnlyList<CambioOperativoDto> CambiosOperativos);
