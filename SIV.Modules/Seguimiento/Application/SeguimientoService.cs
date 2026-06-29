using SIV.Shared.Contracts;

namespace SIV.Modules.Seguimiento.Application;

internal class SeguimientoService : ISeguimientoService, ISeguimientoConsulta
{
    private readonly ISeguimientoRepository _repo;

    public SeguimientoService(ISeguimientoRepository repo)
    {
        _repo = repo;
    }

    public async Task RegistrarSeguimientoAsync(Guid usuarioId, Guid vueloId)
    {
        var existente = await _repo.ObtenerPorUsuarioYVueloAsync(usuarioId, vueloId);

        if (existente != null && existente.Estado == Domain.EstadoSeguimiento.Activo)
            throw new InvalidOperationException(
                "El usuario ya tiene un seguimiento activo para este vuelo.");

        var seguimiento = Domain.Seguimiento.Crear(usuarioId, vueloId);
        await _repo.AgregarAsync(seguimiento);
        await _repo.GuardarCambiosAsync();
    }

    public async Task CancelarSeguimientoAsync(Guid usuarioId, Guid vueloId)
    {
        var seguimiento = await _repo.ObtenerPorUsuarioYVueloAsync(usuarioId, vueloId)
            ?? throw new InvalidOperationException(
                "No existe un seguimiento activo para este usuario y vuelo.");

        seguimiento.Cancelar();
        await _repo.ActualizarAsync(seguimiento);
        await _repo.GuardarCambiosAsync();
    }

    public async Task<IEnumerable<Guid>> ObtenerUsuariosPorVueloAsync(Guid vueloId)
    {
        var seguimientos = await _repo.ObtenerActivosPorVueloAsync(vueloId);
        return seguimientos.Select(s => s.UsuarioId);
    }
}
