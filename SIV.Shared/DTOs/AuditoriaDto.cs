namespace SIV.Shared.DTOs;

public sealed record AuditoriaDto(
    Guid Id,
    string Modulo,
    string Accion,
    string? Detalle,
    string Resultado,
    DateTime FechaHora);
