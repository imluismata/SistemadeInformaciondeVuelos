namespace SIV.Modules.Vuelos.Domain;

public class Adelanto : CambioOperativo
{
    public TimeSpan TiempoReducido { get; private set; }

    public Adelanto(Guid vueloId, string causa, TimeSpan tiempoReducido)
        : base(vueloId, causa)
    {
        if (tiempoReducido <= TimeSpan.Zero)
            throw new ArgumentException("El tiempo reducido de un adelanto debe ser positivo.");

        TiempoReducido = tiempoReducido;
    }

    public override string TipoCambio => "Adelanto";

    // Misma idea que Retraso, pero el horario se mueve hacia atrás en vez de
    // hacia adelante. Nota que NO necesitamos ningún "if (tipo == 'Adelanto')"
    // en ningún otro lado del código: el polimorfismo ya resolvió esa decisión
    // simplemente por el tipo de objeto que se pasó a AplicarSobre.
    public override void AplicarSobre(Vuelo vuelo)
    {
        ValorAnterior = vuelo.HorarioSalida.ToString("o");

        var nuevoHorario = vuelo.HorarioSalida.Subtract(TiempoReducido);
        vuelo.ActualizarHorarioSalida(nuevoHorario);

        ValorNuevo = nuevoHorario.ToString("o");
    }
}