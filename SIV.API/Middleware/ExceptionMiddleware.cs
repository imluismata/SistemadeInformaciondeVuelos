using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using SIV.Shared.Exceptions;

namespace SIV.API.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (VueloNoEncontradoException ex)
        {
            await EscribirRespuesta(context, HttpStatusCode.NotFound, ex.Message);
        }
        catch (TransicionInvalidaException ex)
        {
            await EscribirRespuesta(context, HttpStatusCode.UnprocessableEntity, ex.Message);
        }
        catch (EmailNoConfirmadoException ex)
        {
            await EscribirRespuesta(context, HttpStatusCode.Forbidden, ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            await EscribirRespuesta(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await EscribirRespuesta(context, HttpStatusCode.BadRequest, ex.Message);
        }
        catch (Exception ex)
        {
            // Red de seguridad: cualquier excepción no prevista (fallo de BD, null, etc.)
            // se registra completa en el log del servidor para diagnóstico, pero al
            // cliente solo le llega un mensaje genérico — nunca el stack trace ni datos
            // internos (evita fuga de información).
            logger.LogError(ex, "Error no controlado procesando {Metodo} {Ruta}.",
                context.Request.Method, context.Request.Path);

            await EscribirRespuesta(context, HttpStatusCode.InternalServerError,
                "Ocurrió un error inesperado. Intenta de nuevo más tarde.");
        }
    }

    private static async Task EscribirRespuesta(HttpContext context, HttpStatusCode status, string mensaje)
    {
        // Si la respuesta ya empezó a escribirse no podemos cambiar el status ni el
        // cuerpo; intentarlo lanzaría otra excepción. En ese caso no hay nada que hacer.
        if (context.Response.HasStarted)
            return;

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var body = JsonSerializer.Serialize(new { error = mensaje });
        await context.Response.WriteAsync(body);
    }
}
