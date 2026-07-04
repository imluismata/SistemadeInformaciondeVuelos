using SIV.Modules.Usuarios.Application.Dtos;
using SIV.Modules.Usuarios.Application.Interfaces;
using SIV.Modules.Usuarios.Domain;

namespace SIV.Modules.Usuarios.Application.Services;

internal class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repo;

    public UsuarioService(IUsuarioRepository repo)
    {
        _repo = repo;
    }

    public async Task CrearAsync(RegistrarUsuarioDto dto)
    {
        var existente = await _repo.ObtenerPorEmailAsync(dto.Email);
        if (existente != null)
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var usuario = Usuario.Crear(dto.Nombre, dto.Email, hash);
        await _repo.AgregarAsync(usuario);
        await _repo.GuardarCambiosAsync();
    }

    public async Task ActualizarAsync(ActualizarUsuarioDto dto)
    {
        var usuario = await _repo.ObtenerPorIdAsync(dto.Id)
            ?? throw new InvalidOperationException("El usuario no existe.");

        usuario.ActualizarNombre(dto.NuevoNombre);
        await _repo.ActualizarAsync(usuario);
        await _repo.GuardarCambiosAsync();
    }

    public async Task EliminarAsync(Guid id)
    {
        var usuario = await _repo.ObtenerPorIdAsync(id)
            ?? throw new InvalidOperationException("El usuario no existe.");

        await _repo.EliminarAsync(usuario);
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

    public async Task<IEnumerable<UsuarioDto>> ObtenerTodosAsync()
    {
        var usuarios = await _repo.ObtenerTodosAsync();
        return usuarios.Select(MapToDto);
    }

    public async Task CambiarRolAsync(CambiarRolUsuarioDto dto)
    {
        var usuario = await _repo.ObtenerPorIdAsync(dto.UsuarioId)
            ?? throw new InvalidOperationException("El usuario no existe.");

        usuario.CambiarRol(dto.NuevoRol);
        await _repo.ActualizarAsync(usuario);
        await _repo.GuardarCambiosAsync();
    }

    public async Task<UsuarioDto?> ValidarCredencialesAsync(string email, string password)
    {
        var usuario = await _repo.ObtenerPorEmailAsync(email);
        if (usuario is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash)) return null;
        return MapToDto(usuario);
    }

    private static UsuarioDto MapToDto(Usuario u) =>
        new(u.Id, u.Nombre, u.Email, u.Rol.ToString(), u.CreadoEn);
}
