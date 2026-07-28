namespace SIV.Intranet.Models;

/// <summary>
/// Métricas agregadas de la lista de vuelos para la fila de KPIs.
/// Se calcula a partir de los datos ya cargados; no consulta nada extra.
/// </summary>
public sealed class ResumenVuelos
{
    public int Total { get; init; }
    public int EnVuelo { get; init; }
    public int Retrasados { get; init; }
    public int Completados { get; init; }

    public static ResumenVuelos DesdeVuelos(IReadOnlyList<VueloApi> vuelos) => new()
    {
        Total = vuelos.Count,
        EnVuelo = vuelos.Count(v => v.EstadoActual == "EnVuelo"),
        Retrasados = vuelos.Count(v => v.EstadoActual == "Retrasado"),
        Completados = vuelos.Count(v => v.EstadoActual == "Completado")
    };
}
