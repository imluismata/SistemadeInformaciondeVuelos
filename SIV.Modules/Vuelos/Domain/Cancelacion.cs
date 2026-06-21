namespace SIV.Modules.Vuelos.Domain;

// Esta es la subclase más importante de revisar porque conecta directamente con
// RN-CO-05 del SRS: "la cancelación de un vuelo es un cambio operativo que
// ADEMÁS desencadena una transición de estado". Fíjate cómo AplicarSobre hace
// ambas cosas en este caso, a diferencia de CambioDePuerta que solo hace una.
public class Cancelacion : CambioOperativo
{
    public Cancelacion(Guid vueloId, string causa)
        : base(vueloId, causa)
    {
    }

    public override string TipoCambio => "Cancelacion";

    public override void AplicarSobre(Vuelo vuelo)
    {
        ValorAnterior = vuelo.EstadoActual.ToString();
        ValorNuevo = EstadoVuelo.Cancelado.ToString();

        // RN-CO-05: el registro del cambio operativo (ya ocurre en Vuelo.cs,
        // quien añade "this" a _cambiosOperativos después de llamar a este
        // método) y la transición de estado son DOS eventos independientes,
        // pero ambos ocurren como consecuencia de esta única llamada.
        // MarcarComoCancelado() internamente llama a AvanzarEstado(), que a su
        // vez valida RN-EO-04 y RN-EO-05 (no se puede cancelar un vuelo que ya
        // está EnVuelo o Aterrizado) y registra el HistorialEstado correspondiente.
        vuelo.MarcarComoCancelado();
    }
}