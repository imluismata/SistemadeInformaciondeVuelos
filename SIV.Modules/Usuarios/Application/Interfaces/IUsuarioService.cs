using SIV.Modules.Usuarios.Application.Base;
using SIV.Modules.Usuarios.Application.Dtos;

namespace SIV.Modules.Usuarios.Application.Interfaces;

public interface IUsuarioService : IBaseService<UsuarioDto, RegistrarUsuarioDto, ActualizarUsuarioDto>
{
    Task<UsuarioDto?> ObtenerPorEmailAsync(string email);
    Task CambiarRolAsync(CambiarRolUsuarioDto dto);
    Task<UsuarioDto?> ValidarCredencialesAsync(string email, string password);
}
