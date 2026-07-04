namespace SIV.Shared.DTOs;

public sealed record CambioOperativoDto(
    Guid Id,
    Guid VueloId,
    string Tipo,
    string Motivo,
    string? ValorAnterior,
    string? ValorNuevo,
    DateTime RegistradoEn);
