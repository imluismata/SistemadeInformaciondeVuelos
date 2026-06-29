namespace SIV.Modules.ConsultaPublica.Application;

public interface IConsultaPublicaService
{
    Task<IEnumerable<VueloPublicoDto>> ObtenerVuelosActivosAsync();
    Task<VueloPublicoDto?> BuscarPorNumeroAsync(string numeroVuelo);
    Task<VueloPublicoDto?> ObtenerDetallePorIdAsync(Guid vueloId);
}
