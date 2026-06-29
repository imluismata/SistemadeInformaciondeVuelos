namespace SIV.Modules.Usuarios.Application.Dtos;

public record UsuarioDto(
    Guid Id,
    string Nombre,
    string Email,
    string Rol,
    DateTime CreadoEn
);
