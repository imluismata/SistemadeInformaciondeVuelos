namespace SIV.Shared.Events;

/// <summary>
/// Contrato que implementa todo módulo interesado en reaccionar ante un cambio
/// en un vuelo (por ejemplo, Notificaciones).
/// Permite incorporar nuevos consumidores sin modificar al publicador (Open/Closed).
/// </summary>
public interface IManejadorVueloCambiado
{
    Task ManejarAsync(IVueloCambiadoEvento evento);
}
