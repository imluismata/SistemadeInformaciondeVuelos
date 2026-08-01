using SIV.Modules.Seguimiento.Application.Dtos;
using SIV.Modules.Seguimiento.Application.Interfaces;
using SIV.Modules.Seguimiento.Domain;
using SIV.Shared.Contracts;

namespace SIV.Modules.Seguimiento.Application.Services;

internal class SeguimientoService : ISeguimientoService, ISeguimientoConsulta
{
    private readonly ISeguimientoRepository _repo;
    private readonly IUsuarioConsulta _usuarios;
    private readonly IVueloConsulta _vuelos;

    public SeguimientoService(ISeguimientoRepository repo, IUsuarioConsulta usuarios, IVueloConsulta vuelos)
    {
        _repo = repo;
        _usuarios = usuarios;
        _vuelos = vuelos;
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

    public async Task<IEnumerable<RegistroSeguimientoDto>> ObtenerTodosAsync()
    {
        var todos = (await _repo.ObtenerTodosAsync()).ToList();

        // Se resuelven correos y números de vuelo en lote (evita N+1).
        var ids = todos.Select(s => s.UsuarioId).Distinct().ToList();
        var correos = (await _usuarios.ObtenerContactosAsync(ids)).ToDictionary(c => c.Id, c => c.Email);
        var numeros = (await _vuelos.ObtenerParaReporteAsync(null, null)).ToDictionary(v => v.Id, v => v.Numero);

        return todos.Select(s => new RegistroSeguimientoDto(
            s.Id,
            s.UsuarioId,
            correos.GetValueOrDefault(s.UsuarioId, "—"),
            s.VueloId,
            numeros.GetValueOrDefault(s.VueloId, "—"),
            s.Estado.ToString(),
            s.CreadoEn,
            s.CanceladoEn));
    }

    // CU-REP-03: cantidad de seguidores activos por vuelo, para el reporte de
    // "vuelos más seguidos". Expuesto por ISeguimientoConsulta (contrato de Shared).
    public async Task<IReadOnlyList<SeguidoresPorVuelo>> ContarSeguidoresActivosAsync()
    {
        var todos = await _repo.ObtenerTodosAsync();
        return todos
            .Where(s => s.Estado == EstadoSeguimiento.Activo)
            .GroupBy(s => s.VueloId)
            .Select(g => new SeguidoresPorVuelo(g.Key, g.Count()))
            .ToList();
    }
}
