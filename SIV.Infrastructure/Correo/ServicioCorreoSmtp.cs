using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using Microsoft.Extensions.Logging;
using SIV.Modules.Usuarios.Application.Interfaces;
using SIV.Shared.Contracts;

namespace SIV.Infrastructure.Correo;

/// <summary>
/// Envía correos por SMTP (por ejemplo, Gmail). Si el envío está deshabilitado
/// en la configuración, registra el mensaje en el log en lugar de enviarlo, para
/// poder desarrollar sin credenciales.
/// </summary>
internal sealed class ServicioCorreoSmtp : IServicioCorreo, INotificadorPorCorreo
{
    // Identificador de la imagen embebida (logo) referenciada en el HTML como cid.
    private const string ContentIdLogo = "logoSiv";

    private readonly OpcionesCorreo _opciones;
    private readonly ILogger<ServicioCorreoSmtp> _logger;

    public ServicioCorreoSmtp(OpcionesCorreo opciones, ILogger<ServicioCorreoSmtp> logger)
    {
        _opciones = opciones;
        _logger = logger;
    }

    public Task EnviarCodigoVerificacionAsync(string destino, string nombre, string codigo)
        => EnviarAsync(
            destino, codigo,
            "Tu código de verificación — Quisqueya Flight Hub",
            ConstruirCuerpo(
                nombre, codigo,
                "Gracias por registrarte. Usa el siguiente código para verificar tu correo:",
                "Si no creaste esta cuenta, ignora este mensaje."));

    public Task EnviarCodigoRecuperacionAsync(string destino, string nombre, string codigo)
        => EnviarAsync(
            destino, codigo,
            "Recupera tu contraseña — Quisqueya Flight Hub",
            ConstruirCuerpo(
                nombre, codigo,
                "Recibimos una solicitud para restablecer tu contraseña. Usa este código:",
                "Si no solicitaste este cambio, ignora este mensaje y tu contraseña seguirá igual."));

    // Notifica por correo a un usuario que sigue un vuelo cuando este cambia.
    public async Task EnviarNotificacionVueloAsync(string destino, string nombre, string numeroVuelo, string mensaje)
    {
        if (!_opciones.Habilitado)
        {
            _logger.LogWarning("Correo deshabilitado. Notificación del vuelo {Vuelo} para {Destino}: {Mensaje}",
                numeroVuelo, destino, mensaje);
            return;
        }

        await EnviarSmtpAsync(
            destino,
            $"Actualización de tu vuelo {numeroVuelo} — Quisqueya Flight Hub",
            ConstruirCuerpoNotificacion(nombre, numeroVuelo, mensaje));
    }

    private async Task EnviarAsync(string destino, string codigo, string asunto, string cuerpoHtml)
    {
        if (!_opciones.Habilitado)
        {
            // Modo desarrollo sin SMTP: el código queda en la consola de la API.
            _logger.LogWarning("Correo deshabilitado. Código para {Destino}: {Codigo}", destino, codigo);
            return;
        }

        await EnviarSmtpAsync(destino, asunto, cuerpoHtml);
    }

    private async Task EnviarSmtpAsync(string destino, string asunto, string cuerpoHtml)
    {
        using var mensaje = new MailMessage
        {
            From = new MailAddress(_opciones.Usuario, _opciones.RemitenteNombre),
            Subject = asunto,
        };
        mensaje.To.Add(destino);

        // El cuerpo se arma como vista HTML para poder incrustar el logo como
        // imagen embebida (cid) que aparece en la firma al pie del correo.
        var vistaHtml = AlternateView.CreateAlternateViewFromString(cuerpoHtml, null, MediaTypeNames.Text.Html);

        var rutaLogo = Path.Combine(AppContext.BaseDirectory, "Assets", "logo.png");
        if (File.Exists(rutaLogo))
        {
            var logo = new LinkedResource(rutaLogo, "image/png")
            {
                ContentId = ContentIdLogo,
                TransferEncoding = TransferEncoding.Base64,
            };
            vistaHtml.LinkedResources.Add(logo);
        }

        mensaje.AlternateViews.Add(vistaHtml);

        using var cliente = new SmtpClient(_opciones.Host, _opciones.Puerto)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(_opciones.Usuario, _opciones.Clave),
        };

        await cliente.SendMailAsync(mensaje);
        _logger.LogInformation("Correo enviado a {Destino}.", destino);
    }

    private static string ConstruirCuerpo(string nombre, string codigo, string intro, string nota) => $@"
<div style='font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; color: #1a2b4a;'>
  <p>Hola {nombre},</p>
  <p>{intro}</p>
  <p style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #0d1f4c; text-align: center; margin: 24px 0;'>{codigo}</p>
  <p style='color: #6b7280; font-size: 13px;'>El código vence en 30 minutos. {nota}</p>

  <div style='margin-top: 36px; text-align: center;'>
    <img src='cid:{ContentIdLogo}' alt='Quisqueya Flight Hub' style='width: 150px; height: auto;' />
    <p style='color: #9aa1ad; font-size: 12px; margin: 10px 0 0;'>
      Este es un correo automático de Quisqueya Flight Hub. Por favor no respondas a este mensaje.
    </p>
  </div>
</div>";

    private static string ConstruirCuerpoNotificacion(string nombre, string numeroVuelo, string mensaje) => $@"
<div style='font-family: Arial, sans-serif; max-width: 480px; margin: 0 auto; color: #1a2b4a;'>
  <p>Hola {nombre},</p>
  <p>Tienes una actualización en el vuelo <strong>{numeroVuelo}</strong> que sigues:</p>
  <p style='font-size: 16px; color: #0d1f4c; background: #eef2fb; border-radius: 8px; padding: 16px; margin: 20px 0;'>{mensaje}</p>
  <p style='color: #6b7280; font-size: 13px;'>Recibes este correo porque sigues este vuelo en Quisqueya Flight Hub.</p>

  <div style='margin-top: 36px; text-align: center;'>
    <img src='cid:{ContentIdLogo}' alt='Quisqueya Flight Hub' style='width: 150px; height: auto;' />
    <p style='color: #9aa1ad; font-size: 12px; margin: 10px 0 0;'>
      Este es un correo automático de Quisqueya Flight Hub. Por favor no respondas a este mensaje.
    </p>
  </div>
</div>";
}
