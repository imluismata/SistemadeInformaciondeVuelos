using SIV.Modules.Usuarios.Domain;

namespace SIV.Modules.Usuarios.Application;

internal interface IUsuarioRepository
{
    Task AgregarAsync(Usuario usuario);
    Task<Usuario?> ObtenerPorIdAsync(Guid id);
    Task<IReadOnlyList<Usuario>> ObtenerPorIdsAsync(IReadOnlyCollection<Guid> ids);
    Task<Usuario?> ObtenerPorEmailAsync(string email);
    Task<IEnumerable<Usuario>> ObtenerTodosAsync();
    Task<(IReadOnlyList<Usuario> Items, int Total)> ObtenerPaginadoAsync(int pagina, int tamano);
    Task ActualizarAsync(Usuario usuario);
    Task EliminarAsync(Usuario usuario);
    Task GuardarCambiosAsync();
}
