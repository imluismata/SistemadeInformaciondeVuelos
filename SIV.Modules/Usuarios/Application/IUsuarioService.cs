using SIV.Modules.Usuarios.Domain;

namespace SIV.Modules.Usuarios.Application;

public interface IUsuarioService
{
    Task RegistrarAsync(string nombre, string email, string password);
    Task<UsuarioDto?> ObtenerPorEmailAsync(string email);
    Task<UsuarioDto?> ObtenerPorIdAsync(Guid id);
    Task CambiarRolAsync(Guid usuarioId, RolUsuario nuevoRol);
    Task<UsuarioDto?> ValidarCredencialesAsync(string email, string password);
}
