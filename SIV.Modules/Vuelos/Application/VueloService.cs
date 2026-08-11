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

    // Margen operativo de ocupación de una puerta alrededor del horario del vuelo (embarque
    // antes de salir / desembarque tras llegar). Es el único parámetro de la regla de
    // solape; se puede promover a configuración si algún aeropuerto necesita ajustarlo.
    private static readonly TimeSpan MargenOcupacionPuerta = TimeSpan.FromMinutes(30);

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

    // Obtiene y valida la existencia de la puerta en el catálogo (vía contrato, DA-02).
    // Devuelve null si el vuelo no lleva puerta asignada.
    private async Task<PuertaResumen?> ObtenerPuertaValidadaAsync(Guid? puertaId)
    {
        if (puertaId is not { } id)
            return null;

        return await _catalogo.ObtenerPuertaAsync(id)
            ?? throw new ArgumentException("La puerta indicada no existe en el catálogo.");
    }

    // Descripción de la puerta para guardarla como snapshot en el vuelo
    // ("A5 · Terminal A" o "Rampa abierta").
    private static string DescripcionPuerta(PuertaResumen puerta)
        => puerta.TerminalNombre is { } terminal ? $"{puerta.Codigo} · {terminal}" : puerta.Codigo;

    /// <summary>
    /// Regla operativa: una puerta física atiende un vuelo a la vez. Devuelve el mensaje de
    /// error si la puerta no está disponible (solape con otro vuelo activo, o el vuelo no
    /// toca la base), o null si está libre. La rampa abierta es posición de capacidad
    /// múltiple (válvula de escape para desvíos/emergencias) y nunca da conflicto.
    /// Devolver el error (en vez de lanzarlo) permite reutilizar la regla tanto en el
    /// registro real como en la previsualización de importaciones, que recoge varios errores.
    /// </summary>
    private Task<string?> ValidarDisponibilidadPuertaAsync(Vuelo vuelo, PuertaResumen puerta)
        => ValidarDisponibilidadPuertaAsync(
            vuelo.PuertaId, vuelo.AeropuertoOrigenId, vuelo.AeropuertoDestinoId,
            vuelo.HorarioSalida, vuelo.HorarioLlegada, puerta, vuelo.Id);

    private async Task<string?> ValidarDisponibilidadPuertaAsync(
        Guid? puertaId, Guid origenId, Guid destinoId, DateTime salida, DateTime llegada,
        PuertaResumen puerta, Guid? excluirVueloId)
    {
        if (puerta.EsRampa)
            return null;

        var baseId = await _catalogo.ObtenerAeropuertoBaseIdAsync();
        if (baseId is null)
            return "No se pudo determinar el aeropuerto base del sistema.";

        var ventana = Vuelo.CalcularVentanaOcupacion(
            puertaId, origenId, destinoId, salida, llegada, baseId.Value, MargenOcupacionPuerta);
        if (ventana is null)
            return "Solo puedes asignar una puerta a vuelos que salen de o llegan a la base (SDQ).";

        var otros = await _repository.ObtenerActivosPorPuertaAsync(puerta.Id, excluirVueloId);
        foreach (var otro in otros)
        {
            if (otro.VentanaOcupacionPuerta(baseId.Value, MargenOcupacionPuerta) is not { } ocupada)
                continue;

            // Dos intervalos se solapan si cada uno empieza antes de que el otro termine.
            // Los horarios son hora local de pizarra, así que se muestran tal cual.
            if (ventana.Value.Inicio < ocupada.Fin && ocupada.Inicio < ventana.Value.Fin)
                return $"La puerta {puerta.Codigo} ya está ocupada por el vuelo {otro.Numero} " +
                       $"de {ocupada.Inicio:HH:mm} a {ocupada.Fin:HH:mm}. Elige otra puerta u horario.";
        }

        return null;
    }

    /// <summary>
    /// Valida un registro de vuelo <b>sin persistir</b> y devuelve la lista de errores (vacía
    /// si es válido). Aplica las mismas reglas que <see cref="RegistrarAsync"/> —que la
    /// reutiliza— para que la previsualización de una importación coincida exactamente con lo
    /// que ocurrirá al guardar.
    /// </summary>
    public async Task<IReadOnlyList<string>> ValidarRegistroAsync(RegistrarVueloCommand command)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(command.Numero))
            errores.Add("El número de vuelo es obligatorio.");
        else if (await _repository.ExisteNumeroParaAerolineaYFechaAsync(command.Numero.Trim(), command.AerolineaId, command.HorarioSalida))
            errores.Add($"Ya existe el vuelo {command.Numero.Trim()} para esa aerolínea en esa fecha.");

        if (!await _catalogo.ExisteAerolineaAsync(command.AerolineaId))
            errores.Add("La aerolínea indicada no existe en el catálogo.");
        if (!await _catalogo.ExisteAeropuertoAsync(command.AeropuertoOrigenId))
            errores.Add("El aeropuerto de origen no existe en el catálogo.");
        if (!await _catalogo.ExisteAeropuertoAsync(command.AeropuertoDestinoId))
            errores.Add("El aeropuerto de destino no existe en el catálogo.");
        if (command.AeropuertoOrigenId != Guid.Empty && command.AeropuertoOrigenId == command.AeropuertoDestinoId)
            errores.Add("El aeropuerto de origen y destino no pueden ser el mismo.");
        if (command.HorarioLlegada <= command.HorarioSalida)
            errores.Add("El horario de llegada debe ser posterior al de salida.");

        if (command.PuertaId is { } pid)
        {
            var puerta = await _catalogo.ObtenerPuertaAsync(pid);
            if (puerta is null)
                errores.Add("La puerta indicada no existe en el catálogo.");
            else
            {
                var error = await ValidarDisponibilidadPuertaAsync(
                    command.PuertaId, command.AeropuertoOrigenId, command.AeropuertoDestinoId,
                    command.HorarioSalida, command.HorarioLlegada, puerta, excluirVueloId: null);
                if (error is not null) errores.Add(error);
            }
        }

        return errores;
    }

    public async Task<IReadOnlyList<VueloDto>> ObtenerTodosAsync()
    {
        var vuelos = await _repository.ObtenerTodosAsync();
        return vuelos.Select(Mapear).ToList();
    }

    public async Task<ResultadoPaginado<VueloDto>> ObtenerPaginadoAsync(int pagina, int tamano)
    {
        // Si mandan valores raros, pongo por defecto, y limito a 100 por pagina
        // para no traer todos los vuelos de una.
        if (pagina < 1) pagina = 1;
        if (tamano < 1) tamano = 20;
        if (tamano > 100) tamano = 100;

        // Pido la pagina al repositorio y paso las entidades a DTOs.
        var (items, total) = await _repository.ObtenerPaginadoAsync(pagina, tamano);
        return new ResultadoPaginado<VueloDto>(items.Select(Mapear).ToList(), total, pagina, tamano);
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
            // Una sola fuente de verdad para las reglas de registro (unicidad, catálogo,
            // horarios y disponibilidad de puerta), compartida con la previsualización.
            var errores = await ValidarRegistroAsync(command);
            if (errores.Count > 0)
                throw new InvalidOperationException(string.Join(" ", errores));

            var puerta = await ObtenerPuertaValidadaAsync(command.PuertaId);

            var vuelo = _domainService.Registrar(
                command.Numero,
                command.AerolineaId,
                command.AeropuertoOrigenId,
                command.AeropuertoDestinoId,
                command.HorarioSalida,
                command.HorarioLlegada,
                command.PuertaId,
                puerta is null ? null : DescripcionPuerta(puerta));

            await _repository.GuardarAsync(vuelo);
            await _auditoria.RegistrarAsync("Vuelos", "RegistrarVuelo", "Exitoso", $"Vuelo {vuelo.Numero} registrado.");
            return Mapear(vuelo);
        });

    public Task<VueloDto> ActualizarDatosAsync(Guid vueloId, ActualizarDatosVueloCommand command)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var vuelo = await ObtenerVueloRequerido(vueloId);

            await ValidarCatalogoAsync(command.AerolineaId, command.AeropuertoOrigenId, command.AeropuertoDestinoId);

            var puerta = await ObtenerPuertaValidadaAsync(command.PuertaId);
            _domainService.ActualizarDatos(
                vuelo,
                command.AerolineaId,
                command.AeropuertoOrigenId,
                command.AeropuertoDestinoId,
                command.HorarioSalida,
                command.HorarioLlegada,
                command.PuertaId,
                puerta is null ? null : DescripcionPuerta(puerta),
                command.Motivo);

            // Se valida el solape con los datos ya aplicados (nueva puerta y horarios),
            // excluyendo el propio vuelo de la comparación.
            if (puerta is not null && await ValidarDisponibilidadPuertaAsync(vuelo, puerta) is { } error)
                throw new InvalidOperationException(error);

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
                    if (command.NuevaPuertaId is not { } nuevaPuertaId)
                        throw new InvalidOperationException("Debe indicar la nueva puerta.");
                    // NuevaPuertaId no es null aquí, así que la puerta se resuelve o se lanza.
                    var nuevaPuerta = (await ObtenerPuertaValidadaAsync(nuevaPuertaId))!;
                    _domainService.RegistrarCambioDePuerta(vuelo, nuevaPuertaId, DescripcionPuerta(nuevaPuerta), command.Motivo);
                    if (await ValidarDisponibilidadPuertaAsync(vuelo, nuevaPuerta) is { } errorPuerta)
                        throw new InvalidOperationException(errorPuerta);
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
