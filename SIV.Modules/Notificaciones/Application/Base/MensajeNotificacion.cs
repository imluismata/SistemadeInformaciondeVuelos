using SIV.Shared.Enums;
using SIV.Shared.Events;

namespace SIV.Modules.Notificaciones.Application.Base;

internal static class MensajeNotificacion
{
    private static readonly Dictionary<TipoCambio, Func<IVueloCambiadoEvento, string>> _constructores =
        new()
        {
            [TipoCambio.Cancelacion]    = e => $"El vuelo fue cancelado. Motivo: {e.Causa}",
            [TipoCambio.Retraso]        = e => $"El vuelo fue retrasado. Causa: {e.Causa}",
            [TipoCambio.Adelanto]       = e => $"El vuelo fue adelantado. Causa: {e.Causa}",
            [TipoCambio.CambioDePuerta] = e => $"El vuelo cambió de puerta. {e.Causa}",
        };

    public static string Construir(IVueloCambiadoEvento evento)
    {
        if (_constructores.TryGetValue(evento.TipoCambio, out var constructor))
            return constructor(evento);

        return $"Actualización en el vuelo: {evento.Causa}";
    }
}
