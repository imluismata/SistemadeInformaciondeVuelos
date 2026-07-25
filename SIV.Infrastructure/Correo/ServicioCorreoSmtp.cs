using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using SIV.Modules.Usuarios.Application.Interfaces;

namespace SIV.Infrastructure.Correo;

/// <summary>
/// Envía correos por SMTP (por ejemplo, Gmail). Si el envío está deshabilitado
/// en la configuración, registra el código en el log en lugar de enviarlo, para
/// poder desarrollar sin credenciales.
/// </summary>
internal sealed class ServicioCorreoSmtp : IServicioCorreo
{
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

    private async Task EnviarAsync(string destino, string codigo, string asunto, string cuerpo)
    {
        if (!_opciones.Habilitado)
        {
            // Modo desarrollo sin SMTP: el código queda en la consola de la API.
            _logger.LogWarning("Correo deshabilitado. Código para {Destino}: {Codigo}", destino, codigo);
            return;
        }

        using var mensaje = new MailMessage
        {
            From = new MailAddress(_opciones.Usuario, _opciones.RemitenteNombre),
            Subject = asunto,
            Body = cuerpo,
            IsBodyHtml = true,
        };
        mensaje.To.Add(destino);

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
  <h2 style='color: #0d1f4c;'>Quisqueya <span style='color:#c0152a;'>Flight Hub</span></h2>
  <p>Hola {nombre},</p>
  <p>{intro}</p>
  <p style='font-size: 32px; font-weight: bold; letter-spacing: 8px; color: #0d1f4c; text-align: center; margin: 24px 0;'>{codigo}</p>
  <p style='color: #6b7280; font-size: 13px;'>El código vence en 30 minutos. {nota}</p>
</div>";
}
