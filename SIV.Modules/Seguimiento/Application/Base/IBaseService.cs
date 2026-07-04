namespace SIV.Modules.Seguimiento.Application.Base;

public interface IBaseService<TDto>
{
    Task<IEnumerable<TDto>> ObtenerTodosAsync();
    Task<TDto?> ObtenerPorIdAsync(Guid id);
}
