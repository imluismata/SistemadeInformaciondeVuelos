using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using SIV.Infrastructure;

namespace SIV.API.Health;

// Con esto reviso si la app esta "sana". Para mi, sana significa que pueda
// conectarse a la base de datos. Un monitor le pega a /health cada cierto tiempo;
// si deja de responder OK, se da cuenta de que esta instancia esta fallando.
public sealed class BaseDatosHealthCheck : IHealthCheck
{
    private readonly SivDbContext _db;

    public BaseDatosHealthCheck(SivDbContext db) => _db = db;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            // Trato de conectarme a la base de datos. Si conecto, esta sana.
            var conecta = await _db.Database.CanConnectAsync(cancellationToken);
            return conecta
                ? HealthCheckResult.Healthy("La aplicación y la base de datos responden.")
                : HealthCheckResult.Unhealthy("No se pudo conectar a la base de datos.");
        }
        catch (Exception ex)
        {
            // Si algo truena al consultar la BD, tambien la marco como no sana.
            return HealthCheckResult.Unhealthy("Error al consultar la base de datos.", ex);
        }
    }
}
