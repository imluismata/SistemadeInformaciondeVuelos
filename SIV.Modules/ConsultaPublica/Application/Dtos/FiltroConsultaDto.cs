namespace SIV.Modules.ConsultaPublica.Application.Dtos;

public enum TipoConsulta { Todos, Salidas, Llegadas }

public record FiltroConsultaDto(
    string? Origen = null,
    string? Destino = null,
    DateTime? Fecha = null,
    TipoConsulta Tipo = TipoConsulta.Todos
);
