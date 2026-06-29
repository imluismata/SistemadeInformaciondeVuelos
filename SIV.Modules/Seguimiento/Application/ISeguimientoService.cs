namespace SIV.Modules.Seguimiento.Application;

public interface ISeguimientoService
{
    Task<IEnumerable<Guid>> ObtenerUsuariosPorVueloAsync(Guid vueloId);
    Task RegistrarSeguimientoAsync(Guid usuarioId, Guid vueloId);
    Task CancelarSeguimientoAsync(Guid usuarioId, Guid vueloId);
}