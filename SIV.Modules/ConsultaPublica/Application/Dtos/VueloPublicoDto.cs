namespace SIV.Modules.ConsultaPublica.Application.Dtos;

/// <summary>
/// Vuelo tal como lo ve el público. Además del nombre de la aerolínea y de los
/// aeropuertos, lleva sus códigos IATA: son los que se muestran en las pantallas
/// del aeropuerto ("MIAMI (MIA)") y los que permiten identificar sin ambigüedad
/// a qué aerolínea o ciudad corresponde cada vuelo, sin tener que adivinarlo a
/// partir del nombre.
/// </summary>
/// <remarks>
/// <see cref="HorarioSalida"/> y <see cref="HorarioLlegada"/> son las horas
/// vigentes: si el vuelo se retrasó, ya llevan el retraso aplicado. Los campos
/// <c>Original</c> traen las horas que tenía el vuelo antes de la primera
/// desviación operativa, y solo vienen con valor cuando el vuelo se movió. Las
/// pantallas muestran las dos: la original tachada y la vigente al lado, que es
/// como lo hace un tablero de aeropuerto real.
/// </remarks>
public record VueloPublicoDto(
    Guid Id,
    string Numero,
    string Aerolinea,
    string AerolineaCodigo,
    string Origen,
    string OrigenCodigo,
    string Destino,
    string DestinoCodigo,
    DateTime HorarioSalida,
    DateTime HorarioLlegada,
    DateTime? HorarioSalidaOriginal,
    DateTime? HorarioLlegadaOriginal,
    string? Puerta,
    string Estado
);
