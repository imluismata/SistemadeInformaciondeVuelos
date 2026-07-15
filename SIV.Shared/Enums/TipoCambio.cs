namespace SIV.Shared.Enums;

public enum TipoCambio
{
    Retraso,
    Adelanto,
    CambioDePuerta,
    Cancelacion,

    // Transición de estado que no corresponde a un cambio operativo
    // (Embarcando, EnVuelo, Aterrizado, Completado).
    CambioDeEstado
}
