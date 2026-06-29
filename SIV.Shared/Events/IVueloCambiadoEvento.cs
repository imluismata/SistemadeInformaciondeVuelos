namespace SIV.Shared.Events;

public interface IVueloCambiadoEvento
{
    Guid VueloId { get; }
    string NumeroVuelo { get; }
    string EstadoAnterior { get; }
    string EstadoNuevo { get; }
    string? TipoCambio { get; }
    DateTime OcurridoEn { get; }
}