using SIV.Modules.ConsultaPublica.Application.Dtos;

namespace SIV.Modules.ConsultaPublica.Application;

public interface IConsultaPublicaRepository
{
    Task<IEnumerable<VueloPublicoDto>> ObtenerVuelosActivosAsync();
    Task<IEnumerable<VueloPublicoDto>> ObtenerSalidasAsync();
    Task<IEnumerable<VueloPublicoDto>> ObtenerLlegadasAsync();
    Task<VueloPublicoDto?> BuscarPorNumeroAsync(string numeroVuelo);
    Task<VueloPublicoDto?> ObtenerPorIdAsync(Guid vueloId);
}
