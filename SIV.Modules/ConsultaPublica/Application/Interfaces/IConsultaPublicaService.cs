using SIV.Modules.ConsultaPublica.Application.Dtos;

namespace SIV.Modules.ConsultaPublica.Application.Interfaces;

public interface IConsultaPublicaService
{
    Task<IEnumerable<VueloPublicoDto>> ObtenerVuelosActivosAsync();
    Task<VueloPublicoDto?> BuscarPorNumeroAsync(string numeroVuelo);
    Task<IEnumerable<VueloPublicoDto>> BuscarConFiltroAsync(FiltroConsultaDto filtro);
    Task<VueloPublicoDto?> ObtenerDetallePorIdAsync(Guid vueloId);
}
