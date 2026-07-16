using SIV.Modules.Usuarios.Domain;

namespace SIV.Modules.Usuarios.Application;

public interface IUsuarioRepository
{
    Task AgregarAsync(Usuario usuario);
    Task<Usuario?> ObtenerPorIdAsync(Guid id);
    Task<Usuario?> ObtenerPorEmailAsync(string email);
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();
    Task ActualizarAsync(Usuario usuario);
    Task EliminarAsync(Usuario usuario);
    Task GuardarCambiosAsync();
}
