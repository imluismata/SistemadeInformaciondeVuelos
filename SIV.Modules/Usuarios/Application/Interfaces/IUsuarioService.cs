using SIV.Modules.Usuarios.Application.Base;
using SIV.Modules.Usuarios.Application.Dtos;

namespace SIV.Modules.Usuarios.Application.Interfaces;

public interface IUsuarioService : IBaseService<UsuarioDto, RegistrarUsuarioDto, ActualizarUsuarioDto>
{
    Task<UsuarioDto?> ObtenerPorEmailAsync(string email);
    Task CambiarRolAsync(CambiarRolUsuarioDto dto);
    Task<UsuarioDto?> ValidarCredencialesAsync(string email, string password);
    Task VerificarCodigoAsync(string email, string codigo);
    Task ReenviarCodigoAsync(string email);
    Task SolicitarRecuperacionAsync(string email);
    Task ValidarCodigoRecuperacionAsync(string email, string codigo);
    Task RestablecerPasswordAsync(string email, string codigo, string nuevaPassword);
}
