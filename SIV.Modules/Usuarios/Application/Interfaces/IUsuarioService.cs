using SIV.Modules.Common;
using SIV.Modules.Usuarios.Application.Dtos;

namespace SIV.Modules.Usuarios.Application.Interfaces;

public interface IUsuarioService : IBaseService<UsuarioDto, RegistrarUsuarioDto, ActualizarUsuarioDto>
{
    Task<UsuarioDto?> ObtenerPorEmailAsync(string email);
    Task CambiarRolAsync(CambiarRolUsuarioDto dto);

    // Alta de personal interno hecha por un administrador (CU-USU-03): crea la
    // cuenta con su rol y el correo ya confirmado. Lanza si el email ya existe.
    Task<UsuarioDto> CrearInternoAsync(CrearUsuarioInternoDto dto);

    // Siembra idempotente del administrador inicial desde configuración: crea el
    // admin solo si aún no existe una cuenta con ese email. Resuelve el arranque
    // en frío (bootstrap) sin inyectar usuarios a mano en la base de datos.
    Task AsegurarAdminInicialAsync(CrearUsuarioInternoDto dto);
    Task<UsuarioDto?> ValidarCredencialesAsync(string email, string password);
    Task VerificarCodigoAsync(string email, string codigo);
    Task ReenviarCodigoAsync(string email);
    Task SolicitarRecuperacionAsync(string email);
    Task ValidarCodigoRecuperacionAsync(string email, string codigo);
    Task RestablecerPasswordAsync(string email, string codigo, string nuevaPassword);
}
