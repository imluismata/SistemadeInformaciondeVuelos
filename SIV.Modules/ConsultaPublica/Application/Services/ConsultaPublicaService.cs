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
        var vuelos = await _repo.ObtenerVuelosActivosAsync();

        if (!string.IsNullOrWhiteSpace(filtro.Origen))
            vuelos = vuelos.Where(v => v.Origen.Contains(filtro.Origen, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(filtro.Destino))
            vuelos = vuelos.Where(v => v.Destino.Contains(filtro.Destino, StringComparison.OrdinalIgnoreCase));

        if (filtro.Fecha.HasValue)
            vuelos = vuelos.Where(v => v.HorarioSalida.Date == filtro.Fecha.Value.Date);

        return vuelos;
    }

    public async Task<VueloPublicoDto?> ObtenerDetallePorIdAsync(Guid vueloId)
        => await _repo.ObtenerPorIdAsync(vueloId);
}
