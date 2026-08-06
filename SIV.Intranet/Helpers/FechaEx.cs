namespace SIV.Intranet.Helpers;

/// <summary>
/// Convierte una fecha/hora en UTC (como la guarda el backend con DateTime.UtcNow)
/// a la hora local de República Dominicana para mostrarla en pantalla.
/// RD usa UTC-4 de forma fija (no tiene horario de verano), por lo que basta un
/// desfase constante y así funciona igual sin importar el sistema operativo.
/// </summary>
public static class FechaEx
{
    private const int OffsetRd = -4;

    public static DateTime AHoraLocal(this DateTime utc) => utc.AddHours(OffsetRd);
}
