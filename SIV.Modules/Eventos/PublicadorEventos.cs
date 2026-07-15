using SIV.Shared.Events;

namespace SIV.Modules.Eventos;

/// <summary>
/// Publicador de eventos de dominio. Recibe por inyección todos los manejadores
/// registrados y les entrega el evento, sin conocer sus implementaciones concretas.
/// Agregar un nuevo consumidor no requiere modificar esta clase (Open/Closed).
/// </summary>
internal sealed class PublicadorEventos : IPublicadorEventos
{
    private readonly IEnumerable<IManejadorVueloCambiado> _manejadores;

    public PublicadorEventos(IEnumerable<IManejadorVueloCambiado> manejadores)
    {
        _manejadores = manejadores;
    }

    public async Task PublicarAsync(IVueloCambiadoEvento evento)
    {
        ArgumentNullException.ThrowIfNull(evento);

        foreach (var manejador in _manejadores)
            await manejador.ManejarAsync(evento);
    }
}
