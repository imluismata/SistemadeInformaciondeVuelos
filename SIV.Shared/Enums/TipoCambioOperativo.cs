namespace SIV.Shared.Enums;

// Tipos de cambio operativo que un operador puede registrar. Vive en SIV.Shared
// porque cruza del módulo Vuelos hacia la API (comandos), respetando DA-02.
public enum TipoCambioOperativo
{
    Retraso,
    Adelanto,
    CambioDePuerta,
    Cancelacion,
    ActualizacionDatos
}
