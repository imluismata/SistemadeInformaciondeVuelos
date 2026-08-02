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

    public UsuariosController(IUsuarioService servicio)
    {
        _servicio = servicio;
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

    // El login vive únicamente en AuthController (POST api/auth/login) para no
    // duplicar la lógica de autenticación (DRY). Este controlador se limita a la
    // gestión de usuarios: registro, verificación, roles y consulta (SRP).

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

    // Solo el admin puede ver la lista de usuarios.
    // Si me mandan un 'tamano', devuelvo solo esa pagina; si no, la lista completa
    // como antes (asi no rompo a nadie que ya la usaba).
    [Authorize(Roles = "Administrador")]
    [HttpGet]
    public async Task<IActionResult> ObtenerTodos([FromQuery] int? pagina, [FromQuery] int? tamano)
    {
        if (tamano is > 0)
            return Ok(await _servicio.ObtenerPaginadoAsync(pagina ?? 1, tamano.Value));

        var usuarios = await _servicio.ObtenerTodosAsync();
        return Ok(usuarios);
    }

    // CU-USU-03: solo un administrador da de alta personal interno (Operador,
    // Auditor u otro Administrador). El registro público (POST /registro) queda
    // reservado a los usuarios normales; este endpoint asigna el rol de entrada.
    [Authorize(Roles = "Administrador")]
    [HttpPost]
    public async Task<IActionResult> CrearInterno([FromBody] CrearUsuarioInternoRequest request)
    {
        if (!Enum.TryParse<RolUsuario>(request.Rol, ignoreCase: true, out var rol)
            || rol is not (RolUsuario.OperadorVuelos or RolUsuario.Administrador or RolUsuario.Auditor))
        {
            return BadRequest("Rol inválido. Valores permitidos: OperadorVuelos, Administrador, Auditor.");
        }

        var creado = await _servicio.CrearInternoAsync(
            new CrearUsuarioInternoDto(request.Nombre, request.Email, request.Password, rol));
        return CreatedAtAction(nameof(ObtenerPorId), new { id = creado.Id }, creado);
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
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    string Nombre,

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    string Password);

public record CambiarRolRequest(
    [Required(ErrorMessage = "El rol es obligatorio.")]
    string Rol);

public record CrearUsuarioInternoRequest(
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
    string Nombre,

    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email,

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    string Password,

    [Required(ErrorMessage = "El rol es obligatorio.")]
    string Rol);

public record VerificarCodigoRequest(
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email,

    [Required(ErrorMessage = "El código es obligatorio.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe tener 6 dígitos.")]
    string Codigo);

public record ReenviarCodigoRequest(
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email);

public record RecuperarPasswordRequest(
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email);

public record RestablecerPasswordRequest(
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El email no tiene un formato válido.")]
    string Email,

    [Required(ErrorMessage = "El código es obligatorio.")]
    [RegularExpression(@"^\d{6}$", ErrorMessage = "El código debe tener 6 dígitos.")]
    string Codigo,

    [Required(ErrorMessage = "La nueva contraseña es obligatoria.")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    string NuevaPassword);
