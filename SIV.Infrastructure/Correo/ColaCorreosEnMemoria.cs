using System.Threading.Channels;
using SIV.Shared.Contracts;

namespace SIV.Infrastructure.Correo;

/// <summary>
/// Implementación de <see cref="IColaCorreos"/> respaldada por un
/// <see cref="Channel{T}"/> en memoria. Se registra como singleton: una única cola
/// compartida entre todos los requests (productores) y el worker (único consumidor).
///
/// Canal ilimitado: encolar nunca bloquea al operador. Como contrapartida, si el
/// proceso se cae con la cola llena, esos correos se pierden. Es aceptable porque el
/// correo es un canal complementario "best-effort": la notificación in-app sí queda
/// persistida de forma transaccional (RNF-TRZ-04). Para garantía ante reinicios haría
/// falta un outbox transaccional, descartado por su costo para el alcance académico.
/// </summary>
internal sealed class ColaCorreosEnMemoria : IColaCorreos
{
    private readonly Channel<TrabajoCorreo> _canal =
        Channel.CreateUnbounded<TrabajoCorreo>(new UnboundedChannelOptions
        {
            SingleReader = true,   // un solo worker drena la cola
            SingleWriter = false,  // múltiples requests pueden encolar a la vez
        });

    public void Encolar(TrabajoCorreo trabajo)
    {
        ArgumentNullException.ThrowIfNull(trabajo);
        // En un canal ilimitado no completado, TryWrite siempre acepta el trabajo.
        _canal.Writer.TryWrite(trabajo);
    }

    public IAsyncEnumerable<TrabajoCorreo> LeerTodosAsync(CancellationToken cancelacion) =>
        _canal.Reader.ReadAllAsync(cancelacion);
}
