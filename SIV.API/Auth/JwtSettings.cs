namespace SIV.API.Auth;

/// <summary>
/// Valores de configuración del token JWT, enlazados desde la sección "Jwt"
/// de la configuración. Se inyecta mediante el patrón Options para que las
/// clases dependan de este objeto y no de IConfiguration directamente.
/// </summary>
public sealed class JwtSettings
{
    public const string Seccion = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public int ExpiracionMinutos { get; init; }
    public string Clave { get; init; } = string.Empty;
}
