using System.Net.Http.Headers;
using System.Security.Claims;

namespace SIV.Intranet.Services;

/// <summary>
/// Interceptor que adjunta el token JWT del usuario autenticado a cada
/// petición saliente hacia la API. El token se guarda como claim en la
/// cookie de sesión de la intranet al iniciar sesión.
/// </summary>
public sealed class TokenHandler : DelegatingHandler
{
    public const string ClaimToken = "jwt";

    private readonly IHttpContextAccessor _contexto;

    public TokenHandler(IHttpContextAccessor contexto)
    {
        _contexto = contexto;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = _contexto.HttpContext?.User?.FindFirstValue(ClaimToken);
        if (!string.IsNullOrEmpty(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        return base.SendAsync(request, cancellationToken);
    }
}
