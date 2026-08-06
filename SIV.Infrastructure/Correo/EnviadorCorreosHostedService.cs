using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SIV.Shared.Contracts;

namespace SIV.Infrastructure.Correo;

/// <summary>
/// Worker en segundo plano que drena la cola de correos (<see cref="IColaCorreos"/>)
/// y los envía por SMTP, fuera del request HTTP y de la transacción de negocio
/// (RNF-REN-03). Corre durante toda la vida de la aplicación.
///
/// Un fallo de SMTP en un correo se registra y se continúa con el siguiente: nunca
/// interrumpe el worker ni afecta al flujo operativo (comportamiento best-effort,
/// coherente con el manejador que encola).
/// </summary>
internal sealed class EnviadorCorreosHostedService : BackgroundService
{
    private readonly IColaCorreos _cola;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EnviadorCorreosHostedService> _logger;

    public EnviadorCorreosHostedService(
        IColaCorreos cola,
        IServiceScopeFactory scopeFactory,
        ILogger<EnviadorCorreosHostedService> logger)
    {
        _cola = cola;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // ReadAllAsync espera sin gastar CPU hasta que haya un correo, y termina
        // limpiamente cuando se cancela el token al apagar la aplicación.
        await foreach (var trabajo in _cola.LeerTodosAsync(stoppingToken))
        {
            try
            {
                // INotificadorPorCorreo está registrado como scoped (comparte el
                // ServicioCorreoSmtp), pero este worker es singleton: abrimos un
                // scope corto por envío para resolverlo correctamente.
                using var scope = _scopeFactory.CreateScope();
                var correo = scope.ServiceProvider.GetRequiredService<INotificadorPorCorreo>();

                await correo.EnviarNotificacionVueloAsync(
                    trabajo.Destino, trabajo.Nombre, trabajo.NumeroVuelo, trabajo.Mensaje);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Fallo al enviar el correo de notificación a {Destino} (vuelo {Vuelo}).",
                    trabajo.Destino, trabajo.NumeroVuelo);
            }
        }
    }
}
