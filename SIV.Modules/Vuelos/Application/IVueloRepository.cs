using SIV.Modules.Vuelos.Domain;

namespace SIV.Modules.Vuelos.Application;

internal interface IVueloRepository
{
    Task<IReadOnlyList<Vuelo>> ObtenerTodosAsync();
    Task<IReadOnlyList<Vuelo>> ConsultarAsync(ConsultarVuelosQuery filtro);
    Task<Vuelo?> ObtenerPorIdAsync(Guid id);
    Task<Vuelo?> ObtenerPorNumeroAsync(string numero);
    Task GuardarAsync(Vuelo vuelo);
}