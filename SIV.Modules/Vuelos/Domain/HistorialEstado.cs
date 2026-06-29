namespace SIV.Modules.Vuelos.Domain;

// Esta clase es deliberadamente simple: no tiene comportamiento, solo datos.
// Eso está bien aquí porque un registro de historial es, por naturaleza, un HECHO
// que ya ocurrió. Los hechos no "hacen" nada, solo se consultan. Por eso no tiene
// métodos de negocio como Vuelo o CambioOperativo, que sí necesitan comportamiento
// porque representan procesos en curso.
public class HistorialEstado
{
    // ── ENCAPSULAMIENTO
    // Todos los setters son privados. Una vez que un registro de historial se crea,
    // RN-EO-07 exige que sea INMUTABLE: nadie debe poder cambiar "qué pasó" después
    // de que ya pasó. Si alguien pudiera hacer
    // historial.EstadoNuevo = EstadoVuelo.Programado, estaría reescribiendo el
    // pasado, lo cual rompe cualquier intento de auditoría real.
    public Guid Id { get; private set; }
    public Guid VueloId { get; private set; }
    public EstadoVuelo EstadoAnterior { get; private set; }
    public EstadoVuelo EstadoNuevo { get; private set; }
    public DateTime OcurridoEn { get; private set; }

    private HistorialEstado() { }

    // Este método es "internal" porque solo Vuelo.cs (dentro del mismo módulo)
    // debe poder crear registros de historial. Si fuera "public", cualquier
    // código de otro módulo podría inventar historial falso para un vuelo sin
    // que el vuelo realmente haya cambiado de estado. Al ser "internal", el
    // compilador garantiza que solo el código dentro de SIV.Modules puede
    // llamar a este método — y en la práctica, solo Vuelo.cs lo hace.
    internal static HistorialEstado Registrar(Guid vueloId, EstadoVuelo anterior, EstadoVuelo nuevo)
    {
        return new HistorialEstado
        {
            Id = Guid.NewGuid(),
            VueloId = vueloId,
            EstadoAnterior = anterior,
            EstadoNuevo = nuevo,
            OcurridoEn = DateTime.UtcNow
        };
    }
}