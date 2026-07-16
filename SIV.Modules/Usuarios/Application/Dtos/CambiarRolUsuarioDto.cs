using SIV.Modules.Usuarios.Domain;

namespace SIV.Modules.Usuarios.Application.Dtos;

public record CambiarRolUsuarioDto(Guid UsuarioId, RolUsuario NuevoRol);
