namespace SIV.Shared.DTOs;

/// <summary>
/// Puerta del catálogo. <see cref="TerminalNombre"/> viene resuelto para mostrarlo
/// directamente (p. ej. "Terminal A"); es nulo cuando la puerta es una rampa abierta.
/// </summary>
public sealed record PuertaDto(
    Guid Id,
    string Codigo,
    Guid? TerminalId,
    string? TerminalNombre,
    bool EsRampa,
    bool Activa);
