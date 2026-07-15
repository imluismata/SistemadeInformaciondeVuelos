namespace SIV.Shared.Events;

/// <summary>
/// Abstracción que utiliza el módulo emisor para publicar un evento de dominio
/// sin conocer qué módulos lo consumen (Dependency Inversion).
/// </summary>
public interface IPublicadorEventos
{
    Task PublicarAsync(IVueloCambiadoEvento evento);
}
