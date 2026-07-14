namespace SIV.Shared.DTOs;

public sealed record AeropuertoDto(
    Guid Id,
    string Codigo,
    string Nombre,
    string Pais,
    bool Activo);
