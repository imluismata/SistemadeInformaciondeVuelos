using Microsoft.Extensions.Logging;
using SIV.Modules.Notificaciones.Application.Base;
using SIV.Shared.Contracts;
using SIV.Shared.Events;

namespace SIV.Modules.Notificaciones.Application.Services;

/// <summary>
/// Segundo consumidor del evento VueloCambiado: además de la notificación in-app
/// (<see cref="ManejadorVueloCambiado"/>), notifica por correo a cada usuario que
/// sigue el vuelo. Agregar este canal no requirió modificar el manejador existente ni
/// el publicador: solo registrar un consumidor más (principio Open/Closed).
///
/// El envío no ocurre aquí: este manejador resuelve los destinatarios y <b>encola</b>
/// los correos en <see cref="IColaCorreos"/>, retornando de inmediato. El worker en
/// segundo plano los envía por SMTP fuera del request y de la transacción atómica del
/// cambio de vuelo (RNF-REN-03: el operador no espera por los envíos de correo).
///
/// Sigue siendo best-effort: si algo falla al encolar, se registra el error pero no se
/// interrumpe el cambio de vuelo ni la notificación in-app.
/// </summary>
internal sealed class ManejadorVueloCambiadoCorreo : IManejadorVueloCambiado
{
    private readonly ISeguimientoConsulta _seguimiento;
    private readonly IUsuarioConsulta _usuarios;
    private readonly IColaCorreos _cola;
    private readonly ILogger<ManejadorVueloCambiadoCorreo> _logger;

    public ManejadorVueloCambiadoCorreo(
        ISeguimientoConsulta seguimiento,
        IUsuarioConsulta usuarios,
        IColaCorreos cola,
        ILogger<ManejadorVueloCambiadoCorreo> logger)
    {
        _seguimiento = seguimiento;
        _usuarios = usuarios;
        _cola = cola;
        _logger = logger;
    }

    public async Task ManejarAsync(IVueloCambiadoEvento evento)
    {
        try
        {
            var ids = (await _seguimiento.ObtenerUsuariosPorVueloAsync(evento.VueloId)).ToList();
            if (ids.Count == 0)
                return;

            var contactos = await _usuarios.ObtenerContactosAsync(ids);
            var mensaje = MensajeNotificacion.Construir(evento);

            // No se envía aquí: se encola y el worker EnviadorCorreosHostedService
            // envía por SMTP en segundo plano. Encolar es en memoria e instantáneo.
            foreach (var contacto in contactos)
                _cola.Encolar(new TrabajoCorreo(contacto.Email, contacto.Nombre, evento.NumeroVuelo, mensaje));
        }
        catch (Exception ex)
        {
            // El correo es complementario: un fallo al encolar no debe afectar el flujo principal.
            _logger.LogError(ex, "No se pudieron encolar los correos de notificación del vuelo {VueloId}.", evento.VueloId);
        }
    }
}
