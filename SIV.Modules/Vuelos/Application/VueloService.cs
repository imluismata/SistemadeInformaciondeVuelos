using SIV.Modules.Vuelos.Domain;
using SIV.Shared.Contracts;
using SIV.Shared.DTOs;
using SIV.Shared.Enums;
using SIV.Shared.Events;
using SIV.Shared.Exceptions;

namespace SIV.Modules.Vuelos.Application;

internal sealed class VueloService : IVueloService
{
    // Traduce el estado alcanzado al tipo de cambio que entiende SIV.Shared.
    // Los estados no listados se publican como CambioDeEstado.
    private static readonly IReadOnlyDictionary<EstadoVuelo, TipoCambio> TiposPorEstado =
        new Dictionary<EstadoVuelo, TipoCambio>
        {
            [EstadoVuelo.Cancelado] = TipoCambio.Cancelacion,
            [EstadoVuelo.Retrasado] = TipoCambio.Retraso
        };

    // Traduce el tipo de cambio operativo del dominio de Vuelos al contrato compartido.
    private static readonly IReadOnlyDictionary<TipoCambioOperativo, TipoCambio> TiposPorCambioOperativo =
        new Dictionary<TipoCambioOperativo, TipoCambio>
        {
            [TipoCambioOperativo.Retraso] = TipoCambio.Retraso,
            [TipoCambioOperativo.Adelanto] = TipoCambio.Adelanto,
            [TipoCambioOperativo.CambioDePuerta] = TipoCambio.CambioDePuerta,
            [TipoCambioOperativo.Cancelacion] = TipoCambio.Cancelacion,
            [TipoCambioOperativo.ActualizacionDatos] = TipoCambio.CambioDeEstado
        };

    private readonly IVueloRepository _repository;
    private readonly IVueloDomainService _domainService;
    private readonly IAuditoriaService _auditoria;
    private readonly IPublicadorEventos _publicador;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICatalogoConsulta _catalogo;

    public VueloService(
        IVueloRepository repository,
        IVueloDomainService domainService,
        IAuditoriaService auditoria,
        IPublicadorEventos publicador,
        IUnitOfWork unitOfWork,
        ICatalogoConsulta catalogo)
    {
        _repository = repository;
        _domainService = domainService;
        _auditoria = auditoria;
        _publicador = publicador;
        _unitOfWork = unitOfWork;
        _catalogo = catalogo;
    }

    // Regla del SAD: la aerolínea, el origen y el destino deben existir en el
    // Catálogo antes de registrar o editar un vuelo. Se consulta al módulo de
    // Catálogo por su contrato compartido (DA-02).
    private async Task ValidarCatalogoAsync(Guid aerolineaId, Guid origenId, Guid destinoId)
    {
        if (!await _catalogo.ExisteAerolineaAsync(aerolineaId))
            throw new ArgumentException("La aerolínea indicada no existe en el catálogo.");

        if (!await _catalogo.ExisteAeropuertoAsync(origenId))
            throw new ArgumentException("El aeropuerto de origen no existe en el catálogo.");

        if (!await _catalogo.ExisteAeropuertoAsync(destinoId))
            throw new ArgumentException("El aeropuerto de destino no existe en el catálogo.");
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

    public Task<VueloDto> RegistrarAsync(RegistrarVueloCommand command)
        // Registro del vuelo + auditoría en una sola transacción: si la auditoría
        // falla, el vuelo no queda guardado a medias.
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var existente = await _repository.ObtenerPorNumeroAsync(command.Numero.Trim());
            if (existente is not null)
                throw new InvalidOperationException($"Ya existe un vuelo con el número {command.Numero.Trim()}.");

            await ValidarCatalogoAsync(command.AerolineaId, command.AeropuertoOrigenId, command.AeropuertoDestinoId);

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
        });

    public Task<VueloDto> ActualizarDatosAsync(Guid vueloId, ActualizarDatosVueloCommand command)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var vuelo = await ObtenerVueloRequerido(vueloId);

            await ValidarCatalogoAsync(command.AerolineaId, command.AeropuertoOrigenId, command.AeropuertoDestinoId);

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
        });

    public Task<VueloDto> CambiarEstadoAsync(Guid vueloId, ActualizarEstadoVueloCommand command)
        // Cambio de estado + auditoría + notificaciones en una sola transacción
        // atómica (DA-04 / RNF-TRZ-04): o se confirman los tres, o no se confirma nada.
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var vuelo = await ObtenerVueloRequerido(vueloId);

            // El estado anterior debe capturarse antes de que el dominio mute la entidad.
            var estadoAnterior = vuelo.EstadoActual;

            _domainService.CambiarEstado(vuelo, command.EstadoNuevo);
            await _repository.GuardarAsync(vuelo);
            await _auditoria.RegistrarAsync("Estados", "CambiarEstado", "Exitoso", $"Vuelo {vuelo.Numero} → {vuelo.EstadoActual}.");

            // El dominio ignora la transición hacia el mismo estado; en ese caso no hay nada que notificar.
            if (estadoAnterior != vuelo.EstadoActual)
            {
                await PublicarCambioAsync(
                    vuelo,
                    estadoAnterior,
                    TiposPorEstado.TryGetValue(vuelo.EstadoActual, out var tipo) ? tipo : TipoCambio.CambioDeEstado,
                    $"El vuelo cambió de {estadoAnterior} a {vuelo.EstadoActual}.");
            }

            return Mapear(vuelo);
        });

    public Task<VueloDto> RegistrarCambioOperativoAsync(Guid vueloId, RegistrarCambioOperativoCommand command)
        // Cambio operativo + posible transición de estado + auditoría + notificaciones
        // en una sola transacción atómica (DA-04 / RNF-TRZ-04). Este es exactamente el
        // flujo que la vista de procesos del SAD describe como atómico.
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var vuelo = await ObtenerVueloRequerido(vueloId);

            // Un cambio operativo puede arrastrar consigo una transición de estado
            // (p. ej. un retraso mueve el vuelo a Retrasado), así que se captura antes.
            var estadoAnterior = vuelo.EstadoActual;

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

            await PublicarCambioAsync(
                vuelo,
                estadoAnterior,
                TiposPorCambioOperativo.TryGetValue(command.Tipo, out var tipo) ? tipo : TipoCambio.CambioDeEstado,
                command.Motivo);

            return Mapear(vuelo);
        });

    /// <summary>
    /// Arma el evento de dominio y lo entrega al publicador. VueloService no sabe
    /// qué módulos lo consumen: solo depende de la abstracción IPublicadorEventos.
    /// </summary>
    private Task PublicarCambioAsync(Vuelo vuelo, EstadoVuelo estadoAnterior, TipoCambio tipoCambio, string causa)
        => _publicador.PublicarAsync(new VueloCambiadoEvento(
            vuelo.Id,
            vuelo.Numero,
            estadoAnterior.ToString(),
            vuelo.EstadoActual.ToString(),
            tipoCambio,
            causa,
            DateTime.UtcNow));

    private async Task<Vuelo> ObtenerVueloRequerido(Guid vueloId)
    {
        var vuelo = await _repository.ObtenerPorIdAsync(vueloId);
        if (vuelo is null)
            throw new VueloNoEncontradoException(vueloId);

        return vuelo;
    }

    private static VueloDto Mapear(Vuelo vuelo) => VueloMapper.ADto(vuelo);
}
