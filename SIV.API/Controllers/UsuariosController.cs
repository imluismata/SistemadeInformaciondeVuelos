using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SIV.API.Auth;
using SIV.Modules.Usuarios.Application.Dtos;
using SIV.Modules.Usuarios.Application.Interfaces;
using SIV.Modules.Usuarios.Domain;

namespace SIV.API.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _servicio;
    private readonly IProveedorTokenJwt _tokens;

    public UsuariosController(IUsuarioService servicio, IProveedorTokenJwt tokens)
    {
        _servicio = servicio;
        _tokens = tokens;
    }

    [HttpPost("registro")]
    public async Task<IActionResult> Registrar([FromBody] RegistroUsuarioRequest request)
    {
        // La validación de formato (email válido, longitud de contraseña) la aplica
        // automáticamente [ApiController] a partir de las anotaciones del request.
        await _servicio.CrearAsync(new RegistrarUsuarioDto(request.Nombre, request.Email, request.Password));
        return Created(string.Empty, null);
    }

    [HttpPost("verificar")]
    public async Task<IActionResult> Verificar([FromBody] VerificarCodigoRequest request)
    {
        await _servicio.VerificarCodigoAsync(request.Email, request.Codigo);
        return NoContent();
    }

    [HttpPost("reenviar-codigo")]
    public async Task<IActionResult> ReenviarCodigo([FromBody] ReenviarCodigoRequest request)
    {
        await _servicio.ReenviarCodigoAsync(request.Email);
        return NoContent();
    }

    [HttpPost("recuperar")]
    public async Task<IActionResult> RecuperarPassword([FromBody] RecuperarPasswordRequest request)
    {
        await _servicio.SolicitarRecuperacionAsync(request.Email);
        return NoContent();
    }

    [HttpPost("validar-codigo")]
    public async Task<IActionResult> ValidarCodigo([FromBody] VerificarCodigoRequest request)
    {
        await _servicio.ValidarCodigoRecuperacionAsync(request.Email, request.Codigo);
        return NoContent();
    }

    [HttpPost("restablecer")]
    public async Task<IActionResult> RestablecerPassword([FromBody] RestablecerPasswordRequest request)
    {
        await _servicio.RestablecerPasswordAsync(request.Email, request.Codigo, request.NuevaPassword);
        return NoContent();
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var usuario = await _servicio.ValidarCredencialesAsync(request.Email, request.Password);

        if (usuario is null)
            return Unauthorized("Email o contraseña incorrectos.");

        // Se emite el token JWT para que el portal pueda autenticar sus llamadas
        // a seguimiento y notificaciones (RNF-SEG-01).
        var token = _tokens.GenerarToken(usuario);
        return Ok(new LoginResponse(token, usuario));
    }

    // RNF-SEG-03: los datos personales solo los consulta el propio usuario o un administrador.
    [Authorize]
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> ObtenerPorId(Guid id)
    {
        if (!User.EsAdministrador() && User.ObtenerUsuarioId() != id)
            return Forbid();

        var usuario = await _servicio.ObtenerPorIdAsync(id);
        if (usuario is null) return NotFound($"No se encontró un usuario con Id {id}.");
        return Ok(usuario);
    }

    // CU-USU-03: la gestión de usuarios internos es exclusiva del administrador.
    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos()
    {
        var usuarios = await _servicio.ObtenerTodosAsync();
        return Ok(usuarios);
    }

    // RNF-SEG-02: cambiar el rol es una operación de administrador; impide la escalada de privilegios.
    [Authorize(Roles = "Administrador")]
    [HttpPatch("{id:guid}/rol")]
    public async Task<IActionResult> CambiarRol(Guid id, [FromBody] CambiarRolRequest request)
    {
        if (!Enum.TryParse<RolUsuario>(request.Rol, ignoreCase: true, out var rol))
            return BadRequest($"Rol inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<RolUsuario>())}");

        await _servicio.CambiarRolAsync(new CambiarRolUsuarioDto(id, rol));
        return NoContent();
    }

    // CU-USU-03: desactivar/eliminar cuentas es exclusivo del administrador.
    [Authorize(Roles = "Administrador")]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }
}

public record RegistroUsuarioRequest(
    [property: Required(ErrorMessage = "El nombre es obligatorio.")]
    [property: StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    string Nombre,

    [property: Required(ErrorMessage = "El email es obligatorio.")]
    [property: EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email,

    [property: Required(ErrorMessage = "La contraseña es obligatoria.")]
    [property: StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    string Password);

public record LoginRequest(
    [property: Required(ErrorMessage = "El email es obligatorio.")]
    [property: EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email,

    [property: Required(ErrorMessage = "La contraseña es obligatoria.")]
    string Password);

public record LoginResponse(string Token, UsuarioDto Usuario);

public record CambiarRolRequest(
    [property: Required(ErrorMessage = "El rol es obligatorio.")]
    string Rol);

public record VerificarCodigoRequest(
    [property: Required(ErrorMessage = "El email es obligatorio.")]
    [property: EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email,

    [property: Required(ErrorMessage = "El código es obligatorio.")]
    [property: RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe tener 6 dígitos.")]
    string Codigo);

public record ReenviarCodigoRequest(
    [property: Required(ErrorMessage = "El email es obligatorio.")]
    [property: EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email);

public record RecuperarPasswordRequest(
    [property: Required(ErrorMessage = "El email es obligatorio.")]
    [property: EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email);

public record RestablecerPasswordRequest(
    [property: Required(ErrorMessage = "El email es obligatorio.")]
    [property: EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email,

    [property: Required(ErrorMessage = "El código es obligatorio.")]
    [property: RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe tener 6 dígitos.")]
    string Codigo,

    [property: Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [property: StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    string NuevaPassword);
