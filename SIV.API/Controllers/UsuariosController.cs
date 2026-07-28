using Microsoft.AspNetCore.Mvc;
using SIV.Modules.Usuarios.Application.Dtos;
using SIV.Modules.Usuarios.Application.Interfaces;
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

    [HttpPost("registro")]
    public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
            return BadRequest("Nombre, email y contraseña son obligatorios.");

        await _servicio.CrearAsync(new RegistrarUsuarioDto(request.Nombre, request.Email, request.Password));
        return Created(string.Empty, null);
    }

    [HttpPost("verificar")]
    public async Task<IActionResult> Verificar([FromBody] VerificarCodigoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Codigo))
            return BadRequest("Email y código son obligatorios.");

        await _servicio.VerificarCodigoAsync(request.Email, request.Codigo);
        return NoContent();
    }

    [HttpPost("reenviar-codigo")]
    public async Task<IActionResult> ReenviarCodigo([FromBody] ReenviarCodigoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("El email es obligatorio.");

        await _servicio.ReenviarCodigoAsync(request.Email);
        return NoContent();
    }

    [HttpPost("recuperar")]
    public async Task<IActionResult> RecuperarPassword([FromBody] RecuperarPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("El email es obligatorio.");

        await _servicio.SolicitarRecuperacionAsync(request.Email);
        return NoContent();
    }

    [HttpPost("validar-codigo")]
    public async Task<IActionResult> ValidarCodigo([FromBody] VerificarCodigoRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Codigo))
            return BadRequest("Email y código son obligatorios.");

        await _servicio.ValidarCodigoRecuperacionAsync(request.Email, request.Codigo);
        return NoContent();
    }

    [HttpPost("restablecer")]
    public async Task<IActionResult> RestablecerPassword([FromBody] RestablecerPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Codigo) ||
            string.IsNullOrWhiteSpace(request.NuevaPassword))
            return BadRequest("Email, código y nueva contraseña son obligatorios.");

        await _servicio.RestablecerPasswordAsync(request.Email, request.Codigo, request.NuevaPassword);
        return NoContent();
    }

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

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        var usuario = await _servicio.ObtenerPorIdAsync(id);
        if (usuario is null) return NotFound($"No se encontró un usuario con Id {id}.");
        return Ok(usuario);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var usuarios = await _servicio.ObtenerTodosAsync();
        return Ok(usuarios);
    }

    [HttpPatch("{id:guid}/rol")]
    public async Task<IActionResult> CambiarRol(Guid id, [FromBody] CambiarRolRequest request)
    {
        if (!Enum.TryParse<RolUsuario>(request.Rol, ignoreCase: true, out var rol))
            return BadRequest($"Rol inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<RolUsuario>())}");

        await _servicio.CambiarRolAsync(new CambiarRolUsuarioDto(id, rol));
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }
}

public record RegistroUsuarioRequest(string Nombre, string Email, string Password);
public record LoginRequest(string Email, string Password);
public record CambiarRolRequest(string Rol);
public record VerificarCodigoRequest(string Email, string Codigo);
public record ReenviarCodigoRequest(string Email);
public record RecuperarPasswordRequest(string Email);
public record RestablecerPasswordRequest(string Email, string Codigo, string NuevaPassword);
