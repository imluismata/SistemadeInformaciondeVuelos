using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using SIV.API.Auth;
using SIV.Modules.Usuarios.Application.Dtos;
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
    /// Único punto de autenticación del sistema. Valida credenciales, emite el
    /// token JWT (con el rol dentro) y devuelve los datos del usuario. Lo consumen
    /// tanto la intranet como el portal público, de modo que la lógica de login
    /// vive en un solo lugar (DRY). La validación de credenciales la hace el módulo
    /// Usuarios; este controlador solo orquesta y emite el token (SRP).
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] AuthLoginRequest request)
    {
        // El formato del request lo valida [ApiController] con las anotaciones de abajo.
        var usuario = await _usuarios.ValidarCredencialesAsync(request.Email, request.Password);
        if (usuario is null)
            return Unauthorized("Email o contraseña incorrectos.");

        var token = _tokens.GenerarToken(usuario);
        return Ok(new AuthLoginResponse(token, usuario));
    }
}

public sealed record AuthLoginRequest(
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    string Password);

// Respuesta canónica del login: el token y los datos del usuario autenticado.
public sealed record AuthLoginResponse(string Token, UsuarioDto Usuario);
