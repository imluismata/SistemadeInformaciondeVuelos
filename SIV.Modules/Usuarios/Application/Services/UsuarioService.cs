using SIV.Modules.Usuarios.Application.Dtos;
using SIV.Modules.Usuarios.Application.Interfaces;
using SIV.Modules.Usuarios.Domain;
using SIV.Shared.Exceptions;

namespace SIV.Modules.Usuarios.Application.Services;

internal class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _repo;
    private readonly IServicioCorreo _correo;

    public UsuarioService(IUsuarioRepository repo, IServicioCorreo correo)
    {
        _repo = repo;
        _correo = correo;
    }

    public async Task CrearAsync(RegistrarUsuarioDto dto)
    {
        var existente = await _repo.ObtenerPorEmailAsync(dto.Email);
        if (existente != null)
            throw new InvalidOperationException("Ya existe un usuario con ese email.");

        var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
        var usuario = Usuario.Crear(dto.Nombre, dto.Email, hash);

        // Genera el código y envía el correo antes de persistir; si el correo
        // falla, no queremos dejar una cuenta sin posibilidad de verificarse.
        var codigo = usuario.GenerarCodigoVerificacion();
        await _correo.EnviarCodigoVerificacionAsync(usuario.Email, usuario.Nombre, codigo);

        await _repo.AgregarAsync(usuario);
        await _repo.GuardarCambiosAsync();
    }

    public async Task VerificarCodigoAsync(string email, string codigo)
    {
        var usuario = await _repo.ObtenerPorEmailAsync(email)
            ?? throw new InvalidOperationException("No existe una cuenta con ese correo.");

        usuario.ConfirmarEmail(codigo);
        await _repo.ActualizarAsync(usuario);
        await _repo.GuardarCambiosAsync();
    }

    public async Task ReenviarCodigoAsync(string email)
    {
        var usuario = await _repo.ObtenerPorEmailAsync(email)
            ?? throw new InvalidOperationException("No existe una cuenta con ese correo.");

        if (usuario.EmailConfirmado)
            throw new InvalidOperationException("Esta cuenta ya está verificada.");

        var codigo = usuario.GenerarCodigoVerificacion();
        await _correo.EnviarCodigoVerificacionAsync(usuario.Email, usuario.Nombre, codigo);

        await _repo.ActualizarAsync(usuario);
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

    public async Task SolicitarRecuperacionAsync(string email)
    {
        var usuario = await _repo.ObtenerPorEmailAsync(email);

        // No revelamos si el correo existe o no (evita enumeración de cuentas):
        // se responde igual en ambos casos y solo se envía correo si hay cuenta.
        if (usuario is null)
            return;

        var codigo = usuario.GenerarCodigoRecuperacion();
        await _correo.EnviarCodigoRecuperacionAsync(usuario.Email, usuario.Nombre, codigo);

        await _repo.ActualizarAsync(usuario);
        await _repo.GuardarCambiosAsync();
    }

    public async Task ValidarCodigoRecuperacionAsync(string email, string codigo)
    {
        var usuario = await _repo.ObtenerPorEmailAsync(email)
            ?? throw new InvalidOperationException("No existe una cuenta con ese correo.");

        usuario.ValidarCodigoRecuperacion(codigo);
    }

    public async Task RestablecerPasswordAsync(string email, string codigo, string nuevaPassword)
    {
        var usuario = await _repo.ObtenerPorEmailAsync(email)
            ?? throw new InvalidOperationException("No existe una cuenta con ese correo.");

        var hash = BCrypt.Net.BCrypt.HashPassword(nuevaPassword);
        usuario.RestablecerPassword(codigo, hash);

        await _repo.ActualizarAsync(usuario);
        await _repo.GuardarCambiosAsync();
    }

    public async Task<UsuarioDto?> ValidarCredencialesAsync(string email, string password)
    {
        var usuario = await _repo.ObtenerPorEmailAsync(email);
        if (usuario is null) return null;
        if (!BCrypt.Net.BCrypt.Verify(password, usuario.PasswordHash)) return null;

        // Credenciales correctas pero cuenta sin verificar: se distingue del caso
        // de credenciales inválidas para que el cliente redirija a verificación.
        if (!usuario.EmailConfirmado)
            throw new EmailNoConfirmadoException(usuario.Email);

        return MapToDto(usuario);
    }

    private static UsuarioDto MapToDto(Usuario u) =>
        new(u.Id, u.Nombre, u.Email, u.Rol.ToString(), u.CreadoEn);
}
