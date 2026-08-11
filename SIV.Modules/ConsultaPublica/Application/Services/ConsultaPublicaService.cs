using SIV.Modules.ConsultaPublica.Application.Dtos;
using SIV.Modules.ConsultaPublica.Application.Interfaces;

namespace SIV.Modules.ConsultaPublica.Application.Services;

internal class ConsultaPublicaService : IConsultaPublicaService
{
    private readonly IConsultaPublicaRepository _repo;

    public ConsultaPublicaService(IConsultaPublicaRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<VueloPublicoDto>> ObtenerVuelosActivosAsync()
        => await _repo.ObtenerVuelosActivosAsync();

    public async Task<VueloPublicoDto?> BuscarPorNumeroAsync(string numeroVuelo)
        => await _repo.BuscarPorNumeroAsync(numeroVuelo);

    public async Task<IEnumerable<VueloPublicoDto>> BuscarConFiltroAsync(FiltroConsultaDto filtro)
    {
        // La fecha se pasa al repositorio para que traiga ese día completo en
        // vez de la ventana de 24h en vivo (ver comentario en el repositorio).
        var vuelos = await _repo.ObtenerVuelosActivosAsync(filtro.Fecha);

        if (!string.IsNullOrWhiteSpace(filtro.Origen))
            vuelos = vuelos.Where(v => v.Origen.Contains(filtro.Origen, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(filtro.Destino))
            vuelos = vuelos.Where(v => v.Destino.Contains(filtro.Destino, StringComparison.OrdinalIgnoreCase));

        vuelos = filtro.Tipo switch
        {
            TipoConsulta.Salidas  => vuelos.Where(v => v.HorarioSalida >= DateTime.Now),
            TipoConsulta.Llegadas => vuelos.Where(v => v.HorarioLlegada >= DateTime.Now),
            _                     => vuelos
        };

        return vuelos;
    }

    public async Task<VueloPublicoDto?> ObtenerDetallePorIdAsync(Guid vueloId)
        => await _repo.ObtenerPorIdAsync(vueloId);

    public async Task<IEnumerable<VueloPublicoDto>> ObtenerSalidasAsync(DateTime? fecha = null)
        => await _repo.ObtenerSalidasAsync(fecha);

    public async Task<IEnumerable<VueloPublicoDto>> ObtenerLlegadasAsync(DateTime? fecha = null)
        => await _repo.ObtenerLlegadasAsync(fecha);
}
