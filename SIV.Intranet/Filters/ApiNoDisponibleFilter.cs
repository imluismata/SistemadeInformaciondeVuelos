using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SIV.Intranet.Filters;

/// <summary>
/// Cuando la intranet no logra hablar con la API (caída, timeout o sin red), en vez
/// de reventar con un 500 crudo muestra una página amigable "Servicio no disponible".
/// Cubre el caso "API no disponible" que pide la práctica de la capa de presentación.
/// Solo intercepta fallos de conexión con la API; cualquier otra excepción sigue su
/// curso normal hacia el manejador de errores general.
/// </summary>
public sealed class ApiNoDisponibleFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        // HttpRequestException: la API rechazó la conexión o no responde.
        // TaskCanceledException: se agotó el tiempo de espera del HttpClient.
        var apiCaida = context.Exception is HttpRequestException or TaskCanceledException;
        if (!apiCaida)
            return;

        context.Result = new ViewResult { ViewName = "ServicioNoDisponible" };
        context.HttpContext.Response.StatusCode = 503; // Service Unavailable
        context.ExceptionHandled = true;
    }
}
