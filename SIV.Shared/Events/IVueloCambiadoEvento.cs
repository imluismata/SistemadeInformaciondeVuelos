using SIV.Shared.Enums;

namespace SIV.Shared.Events;

public interface IVueloCambiadoEvento
{
    Guid VueloId { get; }
    TipoCambio TipoCambio { get; }
    string Causa { get; }
    DateTime OcurridoEn { get; }
}
