using SIV.Modules.Seguimiento.Application.Dtos;

namespace SIV.Modules.Seguimiento.Application.Interfaces;

public interface ISeguimientoService
{
    Task RegistrarAsync(RegistrarSeguimientoDto dto);
    Task CancelarAsync(CancelarSeguimientoDto dto);
    Task<IEnumerable<SeguimientoDto>> ObtenerPorUsuarioAsync(Guid usuarioId);
    Task<IEnumerable<Guid>> ObtenerUsuariosPorVueloAsync(Guid vueloId);
    // Historial completo para admin/auditor (CU-SEG-04), con el correo del usuario.
    Task<IEnumerable<RegistroSeguimientoDto>> ObtenerTodosAsync();
}
