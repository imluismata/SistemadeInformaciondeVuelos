namespace SIV.Modules.Usuarios.Application.Dtos;

public record RegistrarUsuarioDto(
    string Nombre,
    string Email,
    string Password
);
