namespace SIV.Modules.ConsultaPublica.Application;

public record VueloPublicoDto(
    Guid Id,
    string Aerolinea,
    string Origen,
    string Destino,
    DateTime HorarioSalida,
    DateTime HorarioLlegada,
    string? Puerta,
    string Estado
);
