namespace SIV.Modules.Vuelos.Domain;

public class CambioDePuerta : CambioOperativo
{
    public string NuevaPuerta { get; private set; } = string.Empty;

    public CambioDePuerta(Guid vueloId, string causa, string nuevaPuerta)
        : base(vueloId, causa)
    {
        if (string.IsNullOrWhiteSpace(nuevaPuerta))
            throw new ArgumentException("La nueva puerta es obligatoria.", nameof(nuevaPuerta));

        NuevaPuerta = nuevaPuerta;
    }

    public override string TipoCambio => "CambioDePuerta";

    public override void AplicarSobre(Vuelo vuelo)
    {
        ValorAnterior = vuelo.Puerta ?? "(sin asignar)";
        vuelo.ActualizarPuerta(NuevaPuerta);
        ValorNuevo = NuevaPuerta;
        // Un cambio de puerta no mueve el vuelo a otro estado operativo:
        // el vuelo sigue Programado o Embarcando, solo cambia su ubicación
        // física. Por eso este método, a diferencia de Retraso, no llama a
        // vuelo.AvanzarEstado(...).
    }
}