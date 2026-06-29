using SIV.Modules.Auditoria.Application;
using SIV.Modules.Vuelos.Domain;
using SIV.Shared.DTOs;
using SIV.Shared.Exceptions;

namespace SIV.Modules.Vuelos.Application;

public sealed class VueloService : IVueloService
{
    private readonly IVueloRepository _repository;
    private readonly IVueloDomainService _domainService;
    private readonly IAuditoriaService _auditoria;

    public VueloService(IVueloRepository repository, IVueloDomainService domainService, IAuditoriaService auditoria)
    {
        _repository = repository;
        _domainService = domainService;
        _auditoria = auditoria;
    }

    public async Task<IReadOnlyList<VueloDto>> ObtenerTodosAsync()
    {
        var vuelos = await _repository.ObtenerTodosAsync();
        return vuelos.Select(Mapear).ToList();
    }

    public async Task<IReadOnlyList<VueloDto>> ConsultarAsync(ConsultarVuelosQuery filtro)
    {
        var vuelos = await _repository.ConsultarAsync(filtro);
        return vuelos.Select(Mapear).ToList();
    }

    public async Task<VueloDto?> ObtenerPorIdAsync(Guid id)
    {
        var vuelo = await _repository.ObtenerPorIdAsync(id);
        return vuelo is null ? null : Mapear(vuelo);
    }

    public async Task<VueloDto> RegistrarAsync(RegistrarVueloCommand command)
    {
        var existente = await _repository.ObtenerPorNumeroAsync(command.Numero.Trim());
        if (existente is not null)
            throw new InvalidOperationException($"Ya existe un vuelo con el número {command.Numero.Trim()}.");

        var vuelo = _domainService.Registrar(
            command.Numero,
            command.AerolineaId,
            command.AeropuertoOrigenId,
            command.AeropuertoDestinoId,
            command.HorarioSalida,
            command.HorarioLlegada,
            command.Puerta);

        await _repository.GuardarAsync(vuelo);
        await _auditoria.RegistrarAsync("Vuelos", "RegistrarVuelo", "Exitoso", $"Vuelo {vuelo.Numero} registrado.");
        return Mapear(vuelo);
    }

    public async Task<VueloDto> ActualizarDatosAsync(Guid vueloId, ActualizarDatosVueloCommand command)
    {
        var vuelo = await ObtenerVueloRequerido(vueloId);
        _domainService.ActualizarDatos(
            vuelo,
            command.AerolineaId,
            command.AeropuertoOrigenId,
            command.AeropuertoDestinoId,
            command.HorarioSalida,
            command.HorarioLlegada,
            command.Puerta,
            command.Motivo);

        await _repository.GuardarAsync(vuelo);
        await _auditoria.RegistrarAsync("Vuelos", "ActualizarDatos", "Exitoso", $"Vuelo {vuelo.Numero} actualizado.");
        return Mapear(vuelo);
    }

    public async Task<VueloDto> CambiarEstadoAsync(Guid vueloId, ActualizarEstadoVueloCommand command)
    {
        var vuelo = await ObtenerVueloRequerido(vueloId);
        _domainService.CambiarEstado(vuelo, command.EstadoNuevo);
        await _repository.GuardarAsync(vuelo);
        await _auditoria.RegistrarAsync("Estados", "CambiarEstado", "Exitoso", $"Vuelo {vuelo.Numero} → {vuelo.EstadoActual}.");
        return Mapear(vuelo);
    }

    public async Task<VueloDto> RegistrarCambioOperativoAsync(Guid vueloId, RegistrarCambioOperativoCommand command)
    {
        var vuelo = await ObtenerVueloRequerido(vueloId);

        switch (command.Tipo)
        {
            case TipoCambioOperativo.Retraso:
                _domainService.RegistrarRetraso(vuelo, command.Duracion ?? TimeSpan.Zero, command.Motivo);
                break;
            case TipoCambioOperativo.Adelanto:
                _domainService.RegistrarAdelanto(vuelo, command.Duracion ?? TimeSpan.Zero, command.Motivo);
                break;
            case TipoCambioOperativo.CambioDePuerta:
                _domainService.RegistrarCambioDePuerta(vuelo, command.NuevaPuerta ?? string.Empty, command.Motivo);
                break;
            case TipoCambioOperativo.Cancelacion:
                _domainService.Cancelar(vuelo, command.Motivo);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command.Tipo), command.Tipo, "Tipo de cambio no soportado.");
        }

        await _repository.GuardarAsync(vuelo);
        await _auditoria.RegistrarAsync("CambiosOperativos", command.Tipo.ToString(), "Exitoso", $"Vuelo {vuelo.Numero}: {command.Motivo}.");
        return Mapear(vuelo);
    }

    private async Task<Vuelo> ObtenerVueloRequerido(Guid vueloId)
    {
        var vuelo = await _repository.ObtenerPorIdAsync(vueloId);
        if (vuelo is null)
            throw new VueloNoEncontradoException(vueloId);

        return vuelo;
    }

    private static VueloDto Mapear(Vuelo vuelo) =>
        new(vuelo.Id,
            vuelo.Numero,
            vuelo.AerolineaId,
            vuelo.AeropuertoOrigenId,
            vuelo.AeropuertoDestinoId,
            vuelo.HorarioSalida,
            vuelo.HorarioLlegada,
            vuelo.Puerta,
            vuelo.EstadoActual.ToString(),
            vuelo.HistorialEstados.Select(h => new HistorialEstadoDto(
                h.Id, h.VueloId, h.EstadoAnterior.ToString(), h.EstadoNuevo.ToString(), h.OcurridoEn)).ToList(),
            vuelo.CambiosOperativos.Select(c => new CambioOperativoDto(
                c.Id, c.VueloId, c.Tipo.ToString(), c.Motivo, c.ValorAnterior, c.ValorNuevo, c.RegistradoEn)).ToList());
}
