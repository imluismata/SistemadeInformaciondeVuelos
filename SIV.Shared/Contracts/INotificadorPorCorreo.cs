namespace SIV.Shared.Contracts;

/// <summary>
/// Abstracción del envío de notificaciones por correo ante cambios de vuelo. La
/// implementación concreta (SMTP) vive en la capa de infraestructura; los módulos
/// solo dependen de este contrato.
/// </summary>
public interface INotificadorPorCorreo
{
    Task EnviarNotificacionVueloAsync(string destino, string nombre, string numeroVuelo, string mensaje);
}
