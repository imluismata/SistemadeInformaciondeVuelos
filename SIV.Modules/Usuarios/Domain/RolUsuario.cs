namespace SIV.Modules.Usuarios.Domain;

// roles que puede tener un usuario en el sistema
public enum RolUsuario
{
    Visitante = 0,
    UsuarioRegistrado = 1,
    OperadorVuelos = 2,
    Administrador = 3,
    Auditor = 4
}