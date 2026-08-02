using SIV.Modules.Vuelos.Domain;
using SIV.Shared.DTOs;

namespace SIV.Modules.Vuelos.Application;

public interface IVueloService
{
    Task<IReadOnlyList<VueloDto>> ObtenerTodosAsync();
    Task<ResultadoPaginado<VueloDto>> ObtenerPaginadoAsync(int pagina, int tamano);
    Task<IReadOnlyList<VueloDto>> ConsultarAsync(ConsultarVuelosQuery filtro);
    Task<VueloDto?> ObtenerPorIdAsync(Guid id);
    Task<VueloDto> RegistrarAsync(RegistrarVueloCommand command);
    Task<VueloDto> ActualizarDatosAsync(Guid vueloId, ActualizarDatosVueloCommand command);
    Task<VueloDto> CambiarEstadoAsync(Guid vueloId, ActualizarEstadoVueloCommand command);
    Task<VueloDto> RegistrarCambioOperativoAsync(Guid vueloId, RegistrarCambioOperativoCommand command);
}