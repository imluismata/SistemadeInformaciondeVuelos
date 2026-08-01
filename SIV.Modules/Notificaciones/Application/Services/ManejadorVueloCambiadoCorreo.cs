using Microsoft.Extensions.Logging;
using SIV.Modules.Notificaciones.Application.Base;
using SIV.Shared.Contracts;
using SIV.Shared.Events;

namespace SIV.Modules.Notificaciones.Application.Services;

/// <summary>
/// Segundo consumidor del evento VueloCambiado: además de la notificación in-app
/// (<see cref="ManejadorVueloCambiado"/>), envía un correo a cada usuario que sigue
/// el vuelo. Agregar este canal no requirió modificar el manejador existente ni el
/// publicador: solo registrar un consumidor más (principio Open/Closed).
///
/// El envío es best-effort: si el SMTP falla, se registra el error pero no se
/// interrumpe el cambio de vuelo ni la notificación in-app.
/// </summary>
internal sealed class ManejadorVueloCambiadoCorreo : IManejadorVueloCambiado
{
    private readonly ISeguimientoConsulta _seguimiento;
    private readonly IUsuarioConsulta _usuarios;
    private readonly INotificadorPorCorreo _correo;
    private readonly ILogger<ManejadorVueloCambiadoCorreo> _logger;

    public ManejadorVueloCambiadoCorreo(
        ISeguimientoConsulta seguimiento,
        IUsuarioConsulta usuarios,
        INotificadorPorCorreo correo,
        ILogger<ManejadorVueloCambiadoCorreo> logger)
    {
        _seguimiento = seguimiento;
        _usuarios = usuarios;
        _correo = correo;
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

            foreach (var contacto in contactos)
                await _correo.EnviarNotificacionVueloAsync(contacto.Email, contacto.Nombre, evento.NumeroVuelo, mensaje);
        }
        catch (Exception ex)
        {
            // El correo es complementario: un fallo de SMTP no debe afectar el flujo principal.
            _logger.LogError(ex, "No se pudieron enviar los correos de notificación del vuelo {VueloId}.", evento.VueloId);
        }
    }
}
