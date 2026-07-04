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
    public string? Puerta { get; internal set; }
    public EstadoVuelo EstadoActual { get; internal set; }
    public DateTime CreadoEn { get; internal set; }

    public IReadOnlyList<HistorialEstado> HistorialEstados => _historialEstados;
    public IReadOnlyList<CambioOperativo> CambiosOperativos => _cambiosOperativos;

    internal static Vuelo Crear() => new();

    internal void AgregarHistorial(HistorialEstado historial) => _historialEstados.Add(historial);

    internal void AgregarCambioOperativo(CambioOperativo cambio) => _cambiosOperativos.Add(cambio);
}
