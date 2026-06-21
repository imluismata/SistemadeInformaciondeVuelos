namespace SIV.Modules.Vuelos.Domain;

// ── HERENCIA: "Retraso ES UN CambioOperativo" ───────────────────────────────
// El ": CambioOperativo" es la sintaxis de herencia en C#. Esto significa que
// Retraso automáticamente tiene Id, VueloId, Causa, ValorAnterior, ValorNuevo
// y Registrado sin tener que volver a escribirlos. Solo agrega lo que le es
// propio y distinto: el TiempoAdicional del retraso.
public class Retraso : CambioOperativo
{
    public TimeSpan TiempoAdicional { get; private set; }

    public Retraso(Guid vueloId, string causa, TimeSpan tiempoAdicional)
        : base(vueloId, causa) // llama al constructor de la clase padre primero
    {
        if (tiempoAdicional <= TimeSpan.Zero)
            throw new ArgumentException("El tiempo adicional de un retraso debe ser positivo.");

        TiempoAdicional = tiempoAdicional;
    }

    // Cada subclase entrega su propio identificador de tipo. Esta es la columna
    // discriminadora que Entity Framework usará en la tabla CambioOperativo.
    public override string TipoCambio => "Retraso";

    // ── AQUÍ ESTÁ LA IMPLEMENTACIÓN CONCRETA DEL POLIMORFISMO ───────────────
    // Esta es la versión de AplicarSobre que se ejecuta específicamente cuando
    // el objeto en tiempo de ejecución es un Retraso. Mueve el horario de
    // salida hacia adelante y deja el vuelo en estado Retrasado.
    public override void AplicarSobre(Vuelo vuelo)
    {
        ValorAnterior = vuelo.HorarioSalida.ToString("o");

        var nuevoHorario = vuelo.HorarioSalida.Add(TiempoAdicional);
        vuelo.ActualizarHorarioSalida(nuevoHorario);

        ValorNuevo = nuevoHorario.ToString("o");

        // El cambio de horario también implica una transición de estado.
        // Esto es la conexión entre RN-CO y RN-EO que documentamos en el SAD.
        if (vuelo.EstadoActual == EstadoVuelo.Programado)
            vuelo.AvanzarEstado(EstadoVuelo.Retrasado);
    }
}