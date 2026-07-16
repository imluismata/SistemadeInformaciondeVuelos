namespace SIV.Modules.Usuarios.Application.Base;

public interface IBaseService<TDto, TCreateDto, TUpdateDto>
{
    Task<IEnumerable<TDto>> ObtenerTodosAsync();
    Task<TDto?> ObtenerPorIdAsync(Guid id);
    Task CrearAsync(TCreateDto dto);
    Task ActualizarAsync(TUpdateDto dto);
    Task EliminarAsync(Guid id);
}
