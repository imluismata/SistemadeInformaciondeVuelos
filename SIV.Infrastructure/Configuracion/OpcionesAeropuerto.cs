namespace SIV.Infrastructure.Configuracion;

/// <summary>
/// Configuración del aeropuerto base del portal. Los tableros de Salidas y
/// Llegadas se calculan respecto a este aeropuerto (por su código IATA).
/// </summary>
public sealed class OpcionesAeropuerto
{
    public const string Seccion = "AeropuertoBase";

    /// <summary>Código IATA del aeropuerto base. Por defecto SDQ (AILA).</summary>
    public string Codigo { get; set; } = "SDQ";
}
