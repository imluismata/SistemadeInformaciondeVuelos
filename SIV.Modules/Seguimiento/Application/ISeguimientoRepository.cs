using SIV.Modules.Seguimiento.Domain;

namespace SIV.Modules.Seguimiento.Application;

public interface ISeguimientoRepository
{
    Task AgregarAsync(Domain.Seguimiento seguimiento);
    Task<Domain.Seguimiento?> ObtenerPorUsuarioYVueloAsync(Guid usuarioId, Guid vueloId);
    Task<IEnumerable<Domain.Seguimiento>> ObtenerActivosPorVueloAsync(Guid vueloId);
    Task<IEnumerable<Domain.Seguimiento>> ObtenerActivosPorUsuarioAsync(Guid usuarioId);
    Task ActualizarAsync(Domain.Seguimiento seguimiento);
    Task GuardarCambiosAsync();
}