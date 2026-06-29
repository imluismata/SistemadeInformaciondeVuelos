using Microsoft.AspNetCore.Mvc;
using SIV.Modules.Usuarios.Application;
using SIV.Modules.Usuarios.Domain;

namespace SIV.API.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _servicio;

    public UsuariosController(IUsuarioService servicio)
    {
        _servicio = servicio;
    }

    // registra un nuevo usuario en el sistema
    [HttpPost("registro")]
    public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Nombre, email y contraseña son obligatorios.");

        await _servicio.RegistrarAsync(request.Nombre, request.Email, request.Password);
        return Created(string.Empty, null);
    }

    // verifica las credenciales y retorna los datos del usuario si son correctas
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Email y contraseña son obligatorios.");

        var usuario = await _servicio.ValidarCredencialesAsync(request.Email, request.Password);

        if (usuario is null)
            return Unauthorized("Email o contraseña incorrectos.");

        return Ok(usuario);
    }

    // retorna el perfil de un usuario por su id
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var usuario = await _servicio.ObtenerPorIdAsync(id);

        if (usuario is null)
            return NotFound($"No se encontró un usuario con Id {id}.");

        return Ok(usuario);
    }

    // cambia el rol de un usuario, solo lo puede hacer un administrador
    [HttpPatch("{id:guid}/rol")]
    public async Task<IActionResult> CambiarRol(Guid id, [FromBody] CambiarRolRequest request)
    {
        if (!Enum.TryParse<RolUsuario>(request.Rol, ignoreCase: true, out var rol))
            return BadRequest($"Rol inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<RolUsuario>())}");

        await _servicio.CambiarRolAsync(id, rol);
        return NoContent();
    }
}

public record RegistroUsuarioRequest(string Nombre, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record CambiarRolRequest(string Rol);
