namespace SIV.Shared.Exceptions;

/// <summary>
/// Se lanza cuando las credenciales son válidas pero el usuario aún no ha
/// confirmado su correo. Permite distinguir este caso de unas credenciales
/// incorrectas para que el cliente redirija a la pantalla de verificación.
/// </summary>
public sealed class EmailNoConfirmadoException : Exception
{
    public string Email { get; }

    public EmailNoConfirmadoException(string email)
        : base("Debes verificar tu correo antes de iniciar sesión.")
    {
        Email = email;
    }
}
