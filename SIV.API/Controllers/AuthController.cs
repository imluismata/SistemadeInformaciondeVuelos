using Microsoft.AspNetCore.Mvc;
using SIV.API.Auth;
using SIV.Modules.Usuarios.Application.Interfaces;

namespace SIV.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IUsuarioService _usuarios;
    private readonly IProveedorTokenJwt _tokens;

    public AuthController(IUsuarioService usuarios, IProveedorTokenJwt tokens)
    {
        _usuarios = usuarios;
        _tokens = tokens;
    }

    /// <summary>
    /// Autentica al usuario y devuelve un token JWT junto con sus datos.
    /// La validación de credenciales la hace el módulo Usuarios; este
    /// controlador solo orquesta y emite el token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email y contraseña son obligatorios.");

        var usuario = await _usuarios.ValidarCredencialesAsync(request.Email, request.Password);
        if (usuario is null)
            return Unauthorized("Email o contraseña incorrectos.");

        var token = _tokens.GenerarToken(usuario);
        return Ok(new AuthLoginResponse(token, usuario.Nombre, usuario.Rol));
    }
}

public sealed record AuthLoginRequest(string Email, string Password);

public sealed record AuthLoginResponse(string Token, string Nombre, string Rol);
