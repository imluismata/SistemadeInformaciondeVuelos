using SIV.Modules.Seguimiento.Application.Dtos;
using SIV.Modules.Seguimiento.Application.Interfaces;
using SIV.Modules.Seguimiento.Domain;
using SIV.Shared.Contracts;

namespace SIV.Modules.Seguimiento.Application.Services;

internal class SeguimientoService : ISeguimientoService, ISeguimientoConsulta
{
    private readonly ISeguimientoRepository _repo;

    public SeguimientoService(ISeguimientoRepository repo)
    {
        _repo = repo;
    }

    public async Task RegistrarAsync(RegistrarSeguimientoDto dto)
    {
        var existente = await _repo.ObtenerPorUsuarioYVueloAsync(dto.UsuarioId, dto.VueloId);

        if (existente != null && existente.Estado == EstadoSeguimiento.Activo)
            throw new InvalidOperationException(
                "El usuario ya tiene un seguimiento activo para este vuelo.");

        var seguimiento = Domain.Seguimiento.Crear(dto.UsuarioId, dto.VueloId);
        await _repo.AgregarAsync(seguimiento);
        await _repo.GuardarCambiosAsync();
    }

    public async Task CancelarAsync(CancelarSeguimientoDto dto)
    {
        var seguimiento = await _repo.ObtenerPorUsuarioYVueloAsync(dto.UsuarioId, dto.VueloId)
            ?? throw new InvalidOperationException(
                "No existe un seguimiento activo para este usuario y vuelo.");

        seguimiento.Cancelar();
        await _repo.ActualizarAsync(seguimiento);
        await _repo.GuardarCambiosAsync();
    }

    public async Task<IEnumerable<SeguimientoDto>> ObtenerPorUsuarioAsync(Guid usuarioId)
    {
        var seguimientos = await _repo.ObtenerActivosPorUsuarioAsync(usuarioId);
        return seguimientos.Select(s => new SeguimientoDto(
            s.Id, s.UsuarioId, s.VueloId, s.Estado.ToString(), s.CreadoEn, s.CanceladoEn));
    }

    public async Task<IEnumerable<Guid>> ObtenerUsuariosPorVueloAsync(Guid vueloId)
    {
        var seguimientos = await _repo.ObtenerActivosPorVueloAsync(vueloId);
        return seguimientos.Select(s => s.UsuarioId);
    }
}
