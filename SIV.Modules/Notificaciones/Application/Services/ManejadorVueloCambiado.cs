using SIV.Modules.Notificaciones.Application.Interfaces;
using SIV.Shared.Events;

namespace SIV.Modules.Notificaciones.Application.Services;

/// <summary>
/// Consumidor del evento de dominio VueloCambiado. Al recibir un cambio de vuelo,
/// delega en el servicio de notificaciones para generar las notificaciones a los
/// usuarios que siguen el vuelo. Es el puente que conecta el módulo de Vuelos
/// (publicador) con el de Notificaciones sin acoplarlos directamente.
/// </summary>
internal sealed class ManejadorVueloCambiado : IManejadorVueloCambiado
{
    private readonly INotificacionService _notificaciones;

    public ManejadorVueloCambiado(INotificacionService notificaciones)
    {
        _notificaciones = notificaciones;
    }

    public Task ManejarAsync(IVueloCambiadoEvento evento)
        => _notificaciones.GenerarNotificacionesAsync(evento);
}
