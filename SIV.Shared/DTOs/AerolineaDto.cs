namespace SIV.Shared.DTOs;

public sealed record AerolineaDto(
    Guid Id,
    string Codigo,
    string Nombre,
    bool Activa);
