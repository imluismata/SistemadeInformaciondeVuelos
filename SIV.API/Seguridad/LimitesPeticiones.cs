using System.Text.Json;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace SIV.API.Seguridad;

/// <summary>
/// Límite de intentos por IP para los endpoints sensibles (RNF-SEG).
///
/// Sin esto, un atacante puede probar contraseñas o códigos de verificación sin
/// freno: el código es de 6 dígitos, así que a fuerza bruta son un millón de
/// combinaciones — cuestión de minutos con un script. Con una ventana fija de
/// pocos intentos por minuto, ese ataque deja de ser viable, mientras que una
/// persona real que se equivoca al escribir ni lo nota.
///
/// Se usa el limitador nativo de .NET 8 (System.Threading.RateLimiting), sin
/// dependencias externas. El estado vive en memoria del proceso: suficiente para
/// un despliegue de una sola instancia como este; con varias instancias haría
/// falta un almacén compartido (Redis), y conviene decirlo antes de que lo
/// pregunten.
/// </summary>
public static class LimitesPeticiones
{
    /// <summary>Inicio de sesión: frena la prueba de contraseñas.</summary>
    public const string Login = "login";

    /// <summary>Registro y códigos de verificación/recuperación por correo.</summary>
    public const string Codigos = "codigos";

    /// <summary>Endpoints anónimos de escritura (seguir vuelos sin cuenta).</summary>
    public const string Anonimo = "anonimo";

    public static IServiceCollection AddLimitesDePeticiones(this IServiceCollection services)
    {
        services.AddRateLimiter(opciones =>
        {
            opciones.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // 5 intentos por minuto: quien se equivoca de contraseña reintenta sin
            // problema; quien automatiza queda cortado enseguida.
            opciones.AddPolicy(Login, PorIp(intentos: 5, ventana: TimeSpan.FromMinutes(1)));

            // Pedir códigos y validarlos comparte el mismo criterio: además de la
            // fuerza bruta, evita que alguien use el sistema para spamear correos.
            opciones.AddPolicy(Codigos, PorIp(intentos: 5, ventana: TimeSpan.FromMinutes(1)));

            // Seguir vuelos sin cuenta es escritura sin autenticar: se deja un margen
            // amplio para el uso normal, pero acotado para que nadie llene la tabla.
            opciones.AddPolicy(Anonimo, PorIp(intentos: 30, ventana: TimeSpan.FromMinutes(1)));

            // Respuesta con el mismo formato de error que usa el resto de la API,
            // para que el portal pueda mostrar el motivo tal cual.
            opciones.OnRejected = async (contexto, cancelacion) =>
            {
                contexto.HttpContext.Response.ContentType = "application/json";

                if (contexto.Lease.TryGetMetadata(MetadataName.RetryAfter, out var esperar))
                    contexto.HttpContext.Response.Headers.RetryAfter =
                        ((int)esperar.TotalSeconds).ToString();

                await contexto.HttpContext.Response.WriteAsync(
                    JsonSerializer.Serialize(new
                    {
                        error = "Demasiados intentos. Espera un momento y vuelve a intentarlo."
                    }),
                    cancelacion);
            };
        });

        return services;
    }

    /// <summary>
    /// Ventana fija por dirección IP. Se reparte por IP y no por usuario porque
    /// justamente hay que frenar al que todavía no se ha identificado.
    /// Nota: detrás de un proxy habría que leer X-Forwarded-For para no meter a
    /// todos los clientes en la misma partición.
    /// </summary>
    private static Func<HttpContext, RateLimitPartition<string>> PorIp(int intentos, TimeSpan ventana)
        => contexto => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: contexto.Connection.RemoteIpAddress?.ToString() ?? "sin-ip",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = intentos,
                Window = ventana,
                QueueLimit = 0, // al pasarse se rechaza, no se encola
            });
}
