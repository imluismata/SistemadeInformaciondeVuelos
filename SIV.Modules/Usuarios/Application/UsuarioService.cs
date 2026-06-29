using SIV.Modules.Usuarios.Domain;

namespace SIV.Modules.Usuarios.Application;

internal class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repo;

    public UsuarioService(IUsuarioRepository repo)
    {
        _repo = repo;
    }

    public async Task RegistrarAsync(string nombre, string email, string password)
    {
        var existente = await _repo.ObtenerPorEmailAsync(email);
        if (existente != null)
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var hash = BCrypt.Net.BCrypt.HashPassword(password);
        var usuario = Usuario.Crear(nombre, email, hash);
        await _repo.AgregarAsync(usuario);
        await _repo.GuardarCambiosAsync();
    }

    public async Task<UsuarioDto?> ObtenerPorEmailAsync(string email)
    {
        var usuario = await _repo.ObtenerPorEmailAsync(email);
        return usuario is null ? null : MapToDto(usuario);
    }

    public async Task<UsuarioDto?> ObtenerPorIdAsync(Guid id)
    {
        var usuario = await _repo.ObtenerPorIdAsync(id);
        return usuario is null ? null : MapToDto(usuario);
    }

    public async Task CambiarRolAsync(Guid usuarioId, RolUsuario nuevoRol)
    {
        var usuario = await _repo.ObtenerPorIdAsync(usuarioId)
            ?? throw new InvalidOperationException("El usuario no existe.");

        usuario.CambiarRol(nuevoRol);
        await _repo.ActualizarAsync(usuario);
        await _repo.GuardarCambiosAsync();
    }

    public async Task<UsuarioDto?> ValidarCredencialesAsync(string email, string password)
    {
        var usuario = await _repo.ObtenerPorEmailAsync(email);

        if (usuario is null)
            return null;

        if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash))
            return null;

        return MapToDto(usuario);
    }

    private static UsuarioDto MapToDto(Usuario usuario) =>
        new(usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString(), usuario.CreadoEn);
}
