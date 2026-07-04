namespace SIV.Modules.ConsultaPublica.Application.Dtos;

public record FiltroConsultaDto(
    string? Origen = null,
    string? Destino = null,
    DateTime? Fecha = null
);
