namespace SIV.Modules.Vuelos.Domain;

public enum TipoCambioOperativo
{
    Retraso,
    Adelanto,
    CambioDePuerta,
    Cancelacion,
    ActualizacionDatos
}

public sealed record CambioOperativo(
    Guid Id,
    Guid VueloId,
    TipoCambioOperativo Tipo,
    string Motivo,
    string? ValorAnterior,
    string? ValorNuevo,
    DateTime RegistradoEn);
