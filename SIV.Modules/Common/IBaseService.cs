namespace SIV.Modules.Common;

/// <summary>
/// Contrato base para los servicios de aplicación con operaciones CRUD estándar.
/// Vive en un único lugar (Common) para no duplicarse por módulo: cualquier
/// servicio que exponga estas operaciones hereda de esta interfaz.
/// </summary>
public interface IBaseService<TDto, TCreateDto, TUpdateDto>
{
    Task<IEnumerable<TDto>> ObtenerTodosAsync();
    Task<TDto?> ObtenerPorIdAsync(Guid id);
    Task CrearAsync(TCreateDto dto);
    Task ActualizarAsync(TUpdateDto dto);
    Task EliminarAsync(Guid id);
}
