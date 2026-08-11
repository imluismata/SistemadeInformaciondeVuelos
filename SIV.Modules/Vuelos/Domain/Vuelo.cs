using SIV.Shared.Enums;

namespace SIV.Modules.Vuelos.Domain;

internal sealed class Vuelo
{
    private readonly List<HistorialEstado> _historialEstados = [];
    private readonly List<CambioOperativo> _cambiosOperativos = [];

    private Vuelo() { }

    public Guid Id { get; internal set; }
    public string Numero { get; internal set; } = string.Empty;
    public Guid AerolineaId { get; internal set; }
    public Guid AeropuertoOrigenId { get; internal set; }
    public Guid AeropuertoDestinoId { get; internal set; }
    public DateTime HorarioSalida { get; internal set; }
    public DateTime HorarioLlegada { get; internal set; }
    // Referencia a la puerta del catálogo (nullable: puede no tener puerta asignada).
    public Guid? PuertaId { get; internal set; }
    // Snapshot del código de la puerta ("A5 · Terminal A", "Rampa abierta") para
    // mostrarlo y para el historial, sin que el dominio dependa del módulo Catálogo.
    public string? PuertaDescripcion { get; internal set; }
    public EstadoVuelo EstadoActual { get; internal set; }
    public DateTime CreadoEn { get; internal set; }

    public IReadOnlyList<HistorialEstado> HistorialEstados => _historialEstados;
    public IReadOnlyList<CambioOperativo> CambiosOperativos => _cambiosOperativos;

    internal static Vuelo Crear() => new();

    /// <summary>
    /// Ventana en la que este vuelo ocupa su puerta en el aeropuerto base:
    /// si <b>sale</b> de la base, desde <paramref name="margen"/> antes de la salida hasta
    /// la salida (embarque → pushback); si <b>llega</b> a la base, desde la llegada hasta
    /// <paramref name="margen"/> después (desembarque). Devuelve <c>null</c> si el vuelo no
    /// tiene puerta asignada o no toca la base — en esos casos no ocupa ninguna puerta suya.
    /// Es cálculo puro: el margen (política) entra como parámetro para no atar el dominio a
    /// configuración.
    /// </summary>
    internal (DateTime Inicio, DateTime Fin)? VentanaOcupacionPuerta(Guid aeropuertoBaseId, TimeSpan margen)
        => CalcularVentanaOcupacion(PuertaId, AeropuertoOrigenId, AeropuertoDestinoId,
            HorarioSalida, HorarioLlegada, aeropuertoBaseId, margen);

    /// <summary>
    /// Misma regla que <see cref="VentanaOcupacionPuerta"/> pero a partir de datos sueltos,
    /// para poder validar una fila de importación antes de construir el vuelo (misma lógica,
    /// un solo lugar).
    /// </summary>
    internal static (DateTime Inicio, DateTime Fin)? CalcularVentanaOcupacion(
        Guid? puertaId, Guid aeropuertoOrigenId, Guid aeropuertoDestinoId,
        DateTime horarioSalida, DateTime horarioLlegada, Guid aeropuertoBaseId, TimeSpan margen)
    {
        if (puertaId is null)
            return null;

        if (aeropuertoOrigenId == aeropuertoBaseId)
            return (horarioSalida - margen, horarioSalida);

        if (aeropuertoDestinoId == aeropuertoBaseId)
            return (horarioLlegada, horarioLlegada + margen);

        return null;
    }

    internal void AgregarHistorial(HistorialEstado historial) => _historialEstados.Add(historial);

    internal void AgregarCambioOperativo(CambioOperativo cambio) => _cambiosOperativos.Add(cambio);
}
