using SIV.Modules.ConsultaPublica.Application;
using SIV.Modules.ConsultaPublica.Application.Dtos;

namespace SIV.Infrastructure.Repositories;

// pendiente: completar las queries cuando Luis suba el dominio de Vuelos
internal class ConsultaPublicaRepository : IConsultaPublicaRepository
{
    private readonly SivDbContext _context;

    public ConsultaPublicaRepository(SivDbContext context)
    {
        _context = context;
    }

    public Task<IEnumerable<VueloPublicoDto>> ObtenerVuelosActivosAsync() =>
        Task.FromResult(Enumerable.Empty<VueloPublicoDto>());

    public Task<VueloPublicoDto?> BuscarPorNumeroAsync(string numeroVuelo) =>
        Task.FromResult<VueloPublicoDto?>(null);

    public Task<VueloPublicoDto?> ObtenerPorIdAsync(Guid vueloId) =>
        Task.FromResult<VueloPublicoDto?>(null);
}
