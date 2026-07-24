namespace SIV.Modules.Vuelos.Domain;

public enum TipoCambioOperativo
{
    Retraso,
    Adelanto,
    CambioDePuerta,
    Cancelacion,
    ActualizacionDatos
}

internal sealed class CambioOperativo
{
    private CambioOperativo() { }

    public CambioOperativo(Guid id, Guid vueloId, TipoCambioOperativo tipo, string motivo, string? valorAnterior, string? valorNuevo, DateTime registradoEn)
    {
        Id = id;
        VueloId = vueloId;
        Tipo = tipo;
        Motivo = motivo;
        ValorAnterior = valorAnterior;
        ValorNuevo = valorNuevo;
        RegistradoEn = registradoEn;
    }

    public Guid Id { get; private set; }
    public Guid VueloId { get; private set; }
    public TipoCambioOperativo Tipo { get; private set; }
    public string Motivo { get; private set; } = string.Empty;
    public string? ValorAnterior { get; private set; }
    public string? ValorNuevo { get; private set; }
    public DateTime RegistradoEn { get; private set; }
}
