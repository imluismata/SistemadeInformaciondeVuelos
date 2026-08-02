using SIV.Modules.Vuelos.Domain;

namespace SIV.Modules.Vuelos.Application;

internal interface IVueloRepository
{
    Task<IReadOnlyList<Vuelo>> ObtenerTodosAsync();
    Task<(IReadOnlyList<Vuelo> Items, int Total)> ObtenerPaginadoAsync(int pagina, int tamano);
    Task<IReadOnlyList<Vuelo>> ConsultarAsync(ConsultarVuelosQuery filtro);
    Task<Vuelo?> ObtenerPorIdAsync(Guid id);
    // Regla del SAD: el número de vuelo es único por aerolínea y fecha (no global).
    Task<bool> ExisteNumeroParaAerolineaYFechaAsync(string numero, Guid aerolineaId, DateTime fecha);
    Task GuardarAsync(Vuelo vuelo);

    // Soporte para la regla de Catálogo (CU-CAT-04): saber si una aerolínea o
    // aeropuerto todavía tiene vuelos no finalizados asociados.
    Task<bool> ExistenVuelosActivosPorAerolineaAsync(Guid aerolineaId);
    Task<bool> ExistenVuelosActivosPorAeropuertoAsync(Guid aeropuertoId);
}