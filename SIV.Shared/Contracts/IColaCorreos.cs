namespace SIV.Shared.Contracts;

/// <summary>
/// Un correo de notificación pendiente de envío, con el destinatario ya resuelto.
/// Es inmutable: representa un trabajo encolado que el worker consumirá tal cual.
/// </summary>
public sealed record TrabajoCorreo(string Destino, string Nombre, string NumeroVuelo, string Mensaje);

/// <summary>
/// Cola de correos de notificación. El productor (el manejador del evento de vuelo)
/// encola sin bloquearse y retorna de inmediato; un worker en segundo plano
/// (<c>EnviadorCorreosHostedService</c>) la consume y envía por SMTP fuera del
/// request HTTP y de la transacción de negocio.
///
/// Esto satisface RNF-REN-03 (notificar cuanto antes sin demorar la operación): el
/// operador ya no espera por los envíos SMTP, y la transacción atómica del cambio de
/// vuelo (DA-04 / RNF-TRZ-04) no se mantiene abierta durante I/O externo lento.
/// </summary>
public interface IColaCorreos
{
    /// <summary>Encola un correo. No bloquea: la escritura es en memoria y retorna al instante.</summary>
    void Encolar(TrabajoCorreo trabajo);

    /// <summary>
    /// Secuencia asíncrona que el worker recorre para drenar la cola. Espera sin
    /// consumir CPU hasta que haya trabajos disponibles.
    /// </summary>
    IAsyncEnumerable<TrabajoCorreo> LeerTodosAsync(CancellationToken cancelacion);
}
