namespace SIV.Shared.DTOs;

public sealed record AuditoriaDto(
    Guid Id,
    string Modulo,
    string Accion,
    string? Detalle,
    string Resultado,
    string Actor,
    DateTime FechaHora);
