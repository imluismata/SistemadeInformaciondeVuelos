using System.Net;
using System.Text.Json;
using SIV.Shared.Exceptions;

namespace SIV.API.Middleware;

public sealed class ExceptionMiddleware(RequestDelegate next)
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
        catch (InvalidOperationException ex)
        {
            await EscribirRespuesta(context, HttpStatusCode.Conflict, ex.Message);
        }
        catch (ArgumentException ex)
        {
            await EscribirRespuesta(context, HttpStatusCode.BadRequest, ex.Message);
        }
    }

    private static async Task EscribirRespuesta(HttpContext context, HttpStatusCode status, string mensaje)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)status;

        var body = JsonSerializer.Serialize(new { error = mensaje });
        await context.Response.WriteAsync(body);
    }
}
