namespace SIV.Modules.Usuarios.Domain;

// clase principal del modulo de usuarios
// la contraseña se guarda como hash, nunca en texto plano
public class Usuario
{
    public Guid Id { get; private set; }
    public string Nombre { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public RolUsuario Rol { get; private set; }
    public DateTime CreadoEn { get; private set; }

    private Usuario() { }

    // unica forma de crear un usuario, aqui se validan los campos obligatorios
    public static Usuario Crear(
        string nombre,
        string email,
        string passwordHash,
        RolUsuario rol = RolUsuario.UsuarioRegistrado)
    {
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre es obligatorio.");

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("El email es obligatorio.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new ArgumentException("El hash de contraseña es obligatorio.");

        return new Usuario
        {
            Id = Guid.NewGuid(),
            Nombre = nombre.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Rol = rol,
            CreadoEn = DateTime.UtcNow
        };
    }

    // solo el administrador deberia poder cambiar el rol, eso se valida en el servicio
    public void CambiarRol(RolUsuario nuevoRol)
    {
        Rol = nuevoRol;
    }
}
