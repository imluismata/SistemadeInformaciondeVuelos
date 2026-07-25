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

    // Verificación de correo: la cuenta no puede iniciar sesión hasta confirmarse.
    public bool EmailConfirmado { get; private set; }
    public string? CodigoVerificacion { get; private set; }
    public DateTime? CodigoExpiraEn { get; private set; }

    private Usuario() { }

    // unica forma de crear un usuario, aqui se validan los campos obligatorios
    public static Usuario Crear(
        string nombre,
        string email,
        string passwordHash,
        RolUsuario rol = RolUsuario.UsuarioRegistrado,
        bool emailConfirmado = false)
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
            CreadoEn = DateTime.UtcNow,
            EmailConfirmado = emailConfirmado
        };
    }

    // Genera un código numérico de 6 dígitos con vigencia de 30 minutos y lo
    // devuelve para que el servicio lo envíe por correo.
    public string GenerarCodigoVerificacion()
    {
        var codigo = Random.Shared.Next(0, 1_000_000).ToString("D6");
        CodigoVerificacion = codigo;
        CodigoExpiraEn = DateTime.UtcNow.AddMinutes(30);
        return codigo;
    }

    // Confirma el correo si el código coincide y no ha expirado.
    public void ConfirmarEmail(string codigo)
    {
        if (EmailConfirmado)
            return;

        ValidarCodigo(codigo);

        EmailConfirmado = true;
        CodigoVerificacion = null;
        CodigoExpiraEn = null;
    }

    // Genera un código para recuperar la contraseña. Reutiliza los mismos campos
    // que la verificación, ya que ambos son códigos temporales de un solo uso.
    public string GenerarCodigoRecuperacion() => GenerarCodigoVerificacion();

    // Comprueba que el código de recuperación sea válido sin consumirlo, para
    // que el cliente pueda confirmar el código antes de pedir la nueva contraseña.
    public void ValidarCodigoRecuperacion(string codigo) => ValidarCodigo(codigo);

    // Cambia la contraseña si el código de recuperación es válido y no expiró.
    public void RestablecerPassword(string codigo, string nuevoPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(nuevoPasswordHash))
            throw new ArgumentException("La nueva contraseña es obligatoria.");

        ValidarCodigo(codigo);

        PasswordHash = nuevoPasswordHash;
        CodigoVerificacion = null;
        CodigoExpiraEn = null;
    }

    // Comprueba que el código pendiente exista, no haya expirado y coincida.
    private void ValidarCodigo(string codigo)
    {
        if (string.IsNullOrWhiteSpace(CodigoVerificacion) || CodigoExpiraEn is null)
            throw new ArgumentException("No hay un código pendiente. Solicita uno nuevo.");

        if (DateTime.UtcNow > CodigoExpiraEn)
            throw new ArgumentException("El código expiró. Solicita uno nuevo.");

        if (!string.Equals(CodigoVerificacion, codigo?.Trim(), StringComparison.Ordinal))
            throw new ArgumentException("El código es incorrecto.");
    }

    // solo el administrador deberia poder cambiar el rol, eso se valida en el servicio
    public void CambiarRol(RolUsuario nuevoRol)
    {
        Rol = nuevoRol;
    }

    public void ActualizarNombre(string nuevoNombre)
    {
        if (string.IsNullOrWhiteSpace(nuevoNombre))
            throw new ArgumentException("El nombre es obligatorio.");

        Nombre = nuevoNombre.Trim();
    }
}
