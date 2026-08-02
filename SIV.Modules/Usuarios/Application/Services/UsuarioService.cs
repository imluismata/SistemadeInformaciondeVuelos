using SIV.Modules.Usuarios.Application.Dtos;
using SIV.Modules.Usuarios.Application.Interfaces;
using SIV.Modules.Usuarios.Domain;
using SIV.Shared.Contracts;
using SIV.Shared.DTOs;
using SIV.Shared.Exceptions;

namespace SIV.Modules.Usuarios.Application.Services;

// Implementa IUsuarioService (gestión) e IUsuarioConsulta (consulta para otros módulos).
internal class UsuarioService : IUsuarioService, IUsuarioConsulta
{
    private readonly IUsuarioRepository _repo;
    private readonly IServicioCorreo _correo;
    private readonly IAuditoriaService _auditoria;
    private readonly IUnitOfWork _unitOfWork;

    public UsuarioService(
        IUsuarioRepository repo,
        IServicioCorreo correo,
        IAuditoriaService auditoria,
        IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _correo = correo;
        _auditoria = auditoria;
        _unitOfWork = unitOfWork;
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

    public Task EliminarAsync(Guid id)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var usuario = await _repo.ObtenerPorIdAsync(id)
                ?? throw new InvalidOperationException("El usuario no existe.");

            await _repo.EliminarAsync(usuario);
            await _repo.GuardarCambiosAsync();
            await _auditoria.RegistrarAsync("Usuarios", "EliminarUsuario", "Exitoso",
                $"Cuenta {usuario.Email} ({usuario.Rol}) eliminada.");
        });

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

    public async Task<ResultadoPaginado<UsuarioDto>> ObtenerPaginadoAsync(int pagina, int tamano)
    {
        // Si me mandan valores raros, pongo unos por defecto. Y no dejo pedir mas
        // de 100 por pagina, para que nadie se traiga toda la tabla de un jalon.
        if (pagina < 1) pagina = 1;
        if (tamano < 1) tamano = 20;
        if (tamano > 100) tamano = 100;

        // Le pido la pagina al repositorio y convierto las entidades a DTOs.
        var (items, total) = await _repo.ObtenerPaginadoAsync(pagina, tamano);
        return new ResultadoPaginado<UsuarioDto>(items.Select(MapToDto).ToList(), total, pagina, tamano);
    }

    public Task CambiarRolAsync(CambiarRolUsuarioDto dto)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var usuario = await _repo.ObtenerPorIdAsync(dto.UsuarioId)
                ?? throw new InvalidOperationException("El usuario no existe.");

            var rolAnterior = usuario.Rol;
            usuario.CambiarRol(dto.NuevoRol);
            await _repo.ActualizarAsync(usuario);
            await _repo.GuardarCambiosAsync();
            await _auditoria.RegistrarAsync("Usuarios", "CambiarRol", "Exitoso",
                $"{usuario.Email}: {rolAnterior} → {dto.NuevoRol}.");
        });

    public Task<UsuarioDto> CrearInternoAsync(CrearUsuarioInternoDto dto)
        // Alta de personal interno + su registro de auditoría en una sola transacción.
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var existente = await _repo.ObtenerPorEmailAsync(dto.Email);
            if (existente != null)
                throw new InvalidOperationException("Ya existe un usuario con ese email.");

            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            // El admin da fe de la cuenta, así que el correo queda confirmado: el
            // personal interno no pasa por el flujo de verificación por email.
            var usuario = Usuario.Crear(dto.Nombre, dto.Email, hash, dto.Rol, emailConfirmado: true);

            await _repo.AgregarAsync(usuario);
            await _repo.GuardarCambiosAsync();
            await _auditoria.RegistrarAsync("Usuarios", "CrearUsuarioInterno", "Exitoso",
                $"Cuenta {usuario.Email} creada con rol {usuario.Rol}.");
            return MapToDto(usuario);
        });

    public async Task AsegurarAdminInicialAsync(CrearUsuarioInternoDto dto)
    {
        // Idempotente: si ya existe una cuenta con ese email, no se hace nada.
        // Así el arranque puede ejecutarse siempre sin duplicar el administrador.
        var existente = await _repo.ObtenerPorEmailAsync(dto.Email);
        if (existente != null)
            return;

        await _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            var admin = Usuario.Crear(dto.Nombre, dto.Email, hash, RolUsuario.Administrador, emailConfirmado: true);

            await _repo.AgregarAsync(admin);
            await _repo.GuardarCambiosAsync();
            // Actor "Sistema": lo genera el arranque, no un usuario autenticado.
            await _auditoria.RegistrarAsync("Usuarios", "SembrarAdminInicial", "Exitoso",
                $"Administrador inicial {admin.Email} creado por el sistema en el arranque.");
        });
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

    // IUsuarioConsulta: datos de contacto de un conjunto de usuarios (para envío de correos).
    public async Task<IReadOnlyList<UsuarioContacto>> ObtenerContactosAsync(IReadOnlyCollection<Guid> ids)
    {
        if (ids.Count == 0)
            return Array.Empty<UsuarioContacto>();

        var usuarios = await _repo.ObtenerPorIdsAsync(ids);
        return usuarios.Select(u => new UsuarioContacto(u.Id, u.Nombre, u.Email)).ToList();
    }

    private static UsuarioDto MapToDto(Usuario u) =>
        new(u.Id, u.Nombre, u.Email, u.Rol.ToString(), u.CreadoEn);
}
