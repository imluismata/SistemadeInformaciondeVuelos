using SIV.Modules.Seguimiento.Application.Dtos;

namespace SIV.Modules.Seguimiento.Application.Interfaces;

public interface ISeguimientoService
{
    Task RegistrarAsync(RegistrarSeguimientoDto dto);
    Task CancelarAsync(CancelarSeguimientoDto dto);
    Task<IEnumerable<SeguimientoDto>> ObtenerPorUsuarioAsync(Guid usuarioId);
    Task<IEnumerable<Guid>> ObtenerUsuariosPorVueloAsync(Guid vueloId);
}
