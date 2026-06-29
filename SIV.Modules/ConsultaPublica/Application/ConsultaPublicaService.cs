namespace SIV.Modules.ConsultaPublica.Application;

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

    public async Task<VueloPublicoDto?> ObtenerDetallePorIdAsync(Guid vueloId)
        => await _repo.ObtenerPorIdAsync(vueloId);
}
