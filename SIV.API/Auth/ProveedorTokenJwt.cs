using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SIV.Modules.Usuarios.Application.Dtos;

namespace SIV.API.Auth;

/// <summary>
/// Genera tokens JWT firmados con HMAC-SHA256. Recibe su configuración por
/// inyección (JwtSettings) para no depender de IConfiguration directamente.
/// </summary>
internal sealed class ProveedorTokenJwt : IProveedorTokenJwt
{
    private readonly JwtSettings _settings;

    public ProveedorTokenJwt(IOptions<JwtSettings> settings)
    {
        _settings = settings.Value;
    }

    public string GenerarToken(UsuarioDto usuario)
    {
        // Claims: identidad del usuario y su rol. El rol se usa luego en
        // [Authorize(Roles = ...)] para autorizar por tipo de actor.
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, usuario.Email),
            new Claim(ClaimTypes.Name, usuario.Nombre),
            new Claim(ClaimTypes.Role, usuario.Rol)
        };

        var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Clave));
        var credenciales = new SigningCredentials(clave, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiracionMinutos),
            signingCredentials: credenciales);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
