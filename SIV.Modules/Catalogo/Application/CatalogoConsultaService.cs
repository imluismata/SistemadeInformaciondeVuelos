using SIV.Shared.Contracts;

namespace SIV.Modules.Catalogo.Application;

/// <summary>
/// Implementación del contrato compartido <see cref="ICatalogoConsulta"/>. Es el
/// punto por el que otros módulos (Vuelos) verifican que una aerolínea o aeropuerto
/// existe, sin conocer las entidades internas de Catálogo. Solo lectura, separada de
/// CatalogoService para no mezclar consulta cross-módulo con la escritura (SRP).
/// </summary>
internal sealed class CatalogoConsultaService(ICatalogoRepository repository, string codigoAeropuertoBase)
    : ICatalogoConsulta
{
    public async Task<bool> ExisteAerolineaAsync(Guid aerolineaId)
        => await repository.ObtenerAerolineaPorIdAsync(aerolineaId) is not null;

    public async Task<bool> ExisteAeropuertoAsync(Guid aeropuertoId)
        => await repository.ObtenerAeropuertoPorIdAsync(aeropuertoId) is not null;

    // El código base (p. ej. "SDQ") se inyecta desde la configuración en el arranque; aquí
    // se traduce a Id contra el catálogo, que es quien conoce los aeropuertos.
    public async Task<Guid?> ObtenerAeropuertoBaseIdAsync()
        => (await repository.ObtenerAeropuertoPorCodigoAsync(codigoAeropuertoBase))?.Id;

    public async Task<bool> ExistePuertaAsync(Guid puertaId)
        => await repository.ObtenerPuertaPorIdAsync(puertaId) is not null;

    public Task<PuertaResumen?> ObtenerPuertaAsync(Guid puertaId)
        => ResolverPuertaAsync(repository.ObtenerPuertaPorIdAsync(puertaId));

    public async Task<Guid?> ObtenerAerolineaIdPorCodigoAsync(string codigo)
        => (await repository.ObtenerAerolineaPorCodigoAsync(codigo.Trim()))?.Id;

    public async Task<Guid?> ObtenerAeropuertoIdPorCodigoAsync(string codigo)
        => (await repository.ObtenerAeropuertoPorCodigoAsync(codigo.Trim()))?.Id;

    public Task<PuertaResumen?> ObtenerPuertaPorCodigoAsync(string codigo)
        => ResolverPuertaAsync(repository.ObtenerPuertaPorCodigoAsync(codigo.Trim()));

    // Arma el resumen de una puerta (código + nombre de su terminal, o rampa) a partir de
    // la tarea que la obtiene, para no duplicar la resolución del terminal (DRY).
    private async Task<PuertaResumen?> ResolverPuertaAsync(Task<Domain.Puerta?> obtener)
    {
        var puerta = await obtener;
        if (puerta is null)
            return null;

        var terminal = puerta.TerminalId is { } tid
            ? await repository.ObtenerTerminalPorIdAsync(tid)
            : null;

        return new PuertaResumen(puerta.Id, puerta.Codigo, terminal?.Nombre, puerta.EsRampa);
    }
}
