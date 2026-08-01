using SIV.Modules.Usuarios.Domain;

namespace SIV.Modules.Usuarios.Application.Dtos;

// Alta de personal interno (Operador, Auditor, Administrador) hecha por un
// administrador: a diferencia del registro público, el rol se asigna de entrada
// y el correo queda confirmado (no pasa por verificación por email).
public record CrearUsuarioInternoDto(
    string Nombre,
    string Email,
    string Password,
    RolUsuario Rol
);
