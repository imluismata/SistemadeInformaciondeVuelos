namespace SIV.Shared.DTOs;

public sealed record TerminalDto(
    Guid Id,
    string Codigo,
    string Nombre,
    Guid AeropuertoId,
    bool Activa);
