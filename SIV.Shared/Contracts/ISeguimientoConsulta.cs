namespace SIV.Shared.Contracts;

public interface ISeguimientoConsulta
{
    Task<IEnumerable<Guid>> ObtenerUsuariosPorVueloAsync(Guid vueloId);
}
