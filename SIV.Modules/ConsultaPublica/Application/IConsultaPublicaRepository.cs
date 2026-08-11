using SIV.Modules.ConsultaPublica.Application.Dtos;

namespace SIV.Modules.ConsultaPublica.Application;

public interface IConsultaPublicaRepository
{
    // Sin fecha: ventana móvil de 24h (el tablero en vivo). Con fecha: ese día
    // completo, sin importar qué hora es "ahora" — quien pide un día concreto no
    // quiere la ventana en vivo.
    Task<IEnumerable<VueloPublicoDto>> ObtenerVuelosActivosAsync(DateTime? fecha = null);
    Task<IEnumerable<VueloPublicoDto>> ObtenerSalidasAsync(DateTime? fecha = null);
    Task<IEnumerable<VueloPublicoDto>> ObtenerLlegadasAsync(DateTime? fecha = null);
    Task<VueloPublicoDto?> BuscarPorNumeroAsync(string numeroVuelo);
    Task<VueloPublicoDto?> ObtenerPorIdAsync(Guid vueloId);
}
