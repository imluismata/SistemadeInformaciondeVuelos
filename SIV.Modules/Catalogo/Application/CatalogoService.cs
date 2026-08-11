using SIV.Modules.Catalogo.Domain;
using SIV.Shared.Contracts;
using SIV.Shared.DTOs;

namespace SIV.Modules.Catalogo.Application;

internal sealed class CatalogoService : ICatalogoService
{
    private readonly ICatalogoRepository _repository;
    private readonly IAuditoriaService _auditoria;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IVueloConsulta _vuelos;

    public CatalogoService(
        ICatalogoRepository repository,
        IAuditoriaService auditoria,
        IUnitOfWork unitOfWork,
        IVueloConsulta vuelos)
    {
        _repository = repository;
        _auditoria = auditoria;
        _unitOfWork = unitOfWork;
        _vuelos = vuelos;
    }

    public async Task<IReadOnlyList<AerolineaDto>> ObtenerAerolineasAsync()
    {
        var aerolineas = await _repository.ObtenerAerolineasAsync();
        return aerolineas.Select(Mapear).ToList();
    }

    public async Task<IReadOnlyList<AeropuertoDto>> ObtenerAeropuertosAsync()
    {
        var aeropuertos = await _repository.ObtenerAeropuertosAsync();
        return aeropuertos.Select(Mapear).ToList();
    }

    public Task<AerolineaDto> RegistrarAerolineaAsync(RegistrarAerolineaCommand command)
        // Alta de la aerolínea + su registro de auditoría en una sola transacción.
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var existente = await _repository.ObtenerAerolineaPorCodigoAsync(command.Codigo.Trim());
            if (existente is not null)
            {
                throw new InvalidOperationException($"Ya existe una aerolínea con el código {command.Codigo.Trim()}.");
            }

            var aerolinea = Aerolinea.Crear(command.Codigo, command.Nombre);
            await _repository.GuardarAerolineaAsync(aerolinea);
            await _auditoria.RegistrarAsync("Catalogo", "RegistrarAerolinea", "Exitoso", $"Aerolínea {aerolinea.Codigo} registrada.");
            return Mapear(aerolinea);
        });

    public Task<AerolineaDto> ActualizarAerolineaAsync(Guid id, ActualizarAerolineaCommand command)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var aerolinea = await ObtenerAerolineaRequeridaAsync(id);
            aerolinea.Actualizar(command.Codigo, command.Nombre);
            await _repository.GuardarAerolineaAsync(aerolinea);
            await _auditoria.RegistrarAsync("Catalogo", "ActualizarAerolinea", "Exitoso", $"Aerolínea {aerolinea.Codigo} actualizada.");
            return Mapear(aerolinea);
        });

    public Task DesactivarAerolineaAsync(Guid id)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var aerolinea = await ObtenerAerolineaRequeridaAsync(id);

            // Regla del SAD (CU-CAT-04): no se puede desactivar una aerolínea con
            // vuelos activos asociados. Se consulta al módulo de Vuelos vía contrato.
            if (await _vuelos.ExistenVuelosActivosParaAerolineaAsync(id))
                throw new InvalidOperationException(
                    $"No se puede desactivar la aerolínea {aerolinea.Codigo}: tiene vuelos activos asociados.");

            aerolinea.Desactivar();
            await _repository.GuardarAerolineaAsync(aerolinea);
            await _auditoria.RegistrarAsync("Catalogo", "DesactivarAerolinea", "Exitoso", $"Aerolínea {aerolinea.Codigo} desactivada.");
        });

    public Task<AeropuertoDto> RegistrarAeropuertoAsync(RegistrarAeropuertoCommand command)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var existente = await _repository.ObtenerAeropuertoPorCodigoAsync(command.Codigo.Trim());
            if (existente is not null)
            {
                throw new InvalidOperationException($"Ya existe un aeropuerto con el código {command.Codigo.Trim()}.");
            }

            var aeropuerto = Aeropuerto.Registrar(command.Codigo, command.Nombre, command.Pais);
            await _repository.GuardarAeropuertoAsync(aeropuerto);
            await _auditoria.RegistrarAsync("Catalogo", "RegistrarAeropuerto", "Exitoso", $"Aeropuerto {aeropuerto.Codigo} registrado.");
            return Mapear(aeropuerto);
        });

    public Task<AeropuertoDto> ActualizarAeropuertoAsync(Guid id, ActualizarAeropuertoCommand command)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var aeropuerto = await ObtenerAeropuertoRequeridoAsync(id);
            aeropuerto.Actualizar(command.Codigo, command.Nombre, command.Pais);
            await _repository.GuardarAeropuertoAsync(aeropuerto);
            await _auditoria.RegistrarAsync("Catalogo", "ActualizarAeropuerto", "Exitoso", $"Aeropuerto {aeropuerto.Codigo} actualizado.");
            return Mapear(aeropuerto);
        });

    public Task ReactivarAerolineaAsync(Guid id)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var aerolinea = await ObtenerAerolineaRequeridaAsync(id);
            aerolinea.Reactivar();
            await _repository.GuardarAerolineaAsync(aerolinea);
            await _auditoria.RegistrarAsync("Catalogo", "ReactivarAerolinea", "Exitoso", $"Aerolínea {aerolinea.Codigo} reactivada.");
        });

    public Task DesactivarAeropuertoAsync(Guid id)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var aeropuerto = await ObtenerAeropuertoRequeridoAsync(id);

            // Regla del SAD (CU-CAT-04): no se puede desactivar un aeropuerto usado
            // como origen o destino por algún vuelo activo.
            if (await _vuelos.ExistenVuelosActivosParaAeropuertoAsync(id))
                throw new InvalidOperationException(
                    $"No se puede desactivar el aeropuerto {aeropuerto.Codigo}: tiene vuelos activos asociados.");

            aeropuerto.Desactivar();
            await _repository.GuardarAeropuertoAsync(aeropuerto);
            await _auditoria.RegistrarAsync("Catalogo", "DesactivarAeropuerto", "Exitoso", $"Aeropuerto {aeropuerto.Codigo} desactivado.");
        });

    public Task ReactivarAeropuertoAsync(Guid id)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var aeropuerto = await ObtenerAeropuertoRequeridoAsync(id);
            aeropuerto.Reactivar();
            await _repository.GuardarAeropuertoAsync(aeropuerto);
            await _auditoria.RegistrarAsync("Catalogo", "ReactivarAeropuerto", "Exitoso", $"Aeropuerto {aeropuerto.Codigo} reactivado.");
        });

    // ==================== Terminales ====================
    public async Task<IReadOnlyList<TerminalDto>> ObtenerTerminalesAsync()
        => (await _repository.ObtenerTerminalesAsync()).Select(MapearTerminal).ToList();

    public Task<TerminalDto> RegistrarTerminalAsync(RegistrarTerminalCommand command)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var aeropuerto = await _repository.ObtenerAeropuertoPorIdAsync(command.AeropuertoId)
                ?? throw new InvalidOperationException("El aeropuerto indicado no existe.");

            var terminales = await _repository.ObtenerTerminalesAsync();
            if (terminales.Any(t => t.AeropuertoId == command.AeropuertoId &&
                                    t.Codigo.Equals(command.Codigo.Trim(), StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException($"Ya existe la terminal {command.Codigo.Trim()} en el aeropuerto {aeropuerto.Codigo}.");

            var terminal = Terminal.Crear(command.Codigo, command.Nombre, command.AeropuertoId);
            await _repository.GuardarTerminalAsync(terminal);
            await _auditoria.RegistrarAsync("Catalogo", "RegistrarTerminal", "Exitoso", $"Terminal {terminal.Codigo} registrada.");
            return MapearTerminal(terminal);
        });

    public Task<TerminalDto> ActualizarTerminalAsync(Guid id, ActualizarTerminalCommand command)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var terminal = await ObtenerTerminalRequeridaAsync(id);
            terminal.Actualizar(command.Codigo, command.Nombre);
            await _repository.GuardarTerminalAsync(terminal);
            await _auditoria.RegistrarAsync("Catalogo", "ActualizarTerminal", "Exitoso", $"Terminal {terminal.Codigo} actualizada.");
            return MapearTerminal(terminal);
        });

    public Task DesactivarTerminalAsync(Guid id)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var terminal = await ObtenerTerminalRequeridaAsync(id);
            terminal.Desactivar();
            await _repository.GuardarTerminalAsync(terminal);
            await _auditoria.RegistrarAsync("Catalogo", "DesactivarTerminal", "Exitoso", $"Terminal {terminal.Codigo} desactivada.");
        });

    public Task ReactivarTerminalAsync(Guid id)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var terminal = await ObtenerTerminalRequeridaAsync(id);
            terminal.Reactivar();
            await _repository.GuardarTerminalAsync(terminal);
            await _auditoria.RegistrarAsync("Catalogo", "ReactivarTerminal", "Exitoso", $"Terminal {terminal.Codigo} reactivada.");
        });

    // ==================== Puertas ====================
    public async Task<IReadOnlyList<PuertaDto>> ObtenerPuertasAsync()
    {
        var puertas = await _repository.ObtenerPuertasAsync();
        var terminales = (await _repository.ObtenerTerminalesAsync()).ToDictionary(t => t.Id, t => t.Nombre);
        return puertas.Select(p => new PuertaDto(
            p.Id, p.Codigo, p.TerminalId,
            p.TerminalId is { } tid && terminales.TryGetValue(tid, out var nombre) ? nombre : null,
            p.EsRampa, p.Activa)).ToList();
    }

    public Task<PuertaDto> RegistrarPuertaAsync(RegistrarPuertaCommand command)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            if (command.TerminalId is { } tid && await _repository.ObtenerTerminalPorIdAsync(tid) is null)
                throw new InvalidOperationException("La terminal indicada no existe.");

            if (await _repository.ObtenerPuertaPorCodigoAsync(command.Codigo.Trim()) is not null)
                throw new InvalidOperationException($"Ya existe una puerta con el código {command.Codigo.Trim()}.");

            var puerta = Puerta.Crear(command.Codigo, command.TerminalId);
            await _repository.GuardarPuertaAsync(puerta);
            await _auditoria.RegistrarAsync("Catalogo", "RegistrarPuerta", "Exitoso", $"Puerta {puerta.Codigo} registrada.");
            return await MapearPuertaAsync(puerta);
        });

    public Task<PuertaDto> ActualizarPuertaAsync(Guid id, ActualizarPuertaCommand command)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var puerta = await ObtenerPuertaRequeridaAsync(id);
            if (command.TerminalId is { } tid && await _repository.ObtenerTerminalPorIdAsync(tid) is null)
                throw new InvalidOperationException("La terminal indicada no existe.");
            puerta.Actualizar(command.Codigo, command.TerminalId);
            await _repository.GuardarPuertaAsync(puerta);
            await _auditoria.RegistrarAsync("Catalogo", "ActualizarPuerta", "Exitoso", $"Puerta {puerta.Codigo} actualizada.");
            return await MapearPuertaAsync(puerta);
        });

    public Task DesactivarPuertaAsync(Guid id)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            // La regla "no desactivar puertas con vuelos activos" se agregará cuando
            // Vuelo referencie PuertaId (feature #4). Por ahora es borrado lógico simple.
            var puerta = await ObtenerPuertaRequeridaAsync(id);
            puerta.Desactivar();
            await _repository.GuardarPuertaAsync(puerta);
            await _auditoria.RegistrarAsync("Catalogo", "DesactivarPuerta", "Exitoso", $"Puerta {puerta.Codigo} desactivada.");
        });

    public Task ReactivarPuertaAsync(Guid id)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var puerta = await ObtenerPuertaRequeridaAsync(id);
            puerta.Reactivar();
            await _repository.GuardarPuertaAsync(puerta);
            await _auditoria.RegistrarAsync("Catalogo", "ReactivarPuerta", "Exitoso", $"Puerta {puerta.Codigo} reactivada.");
        });

    public Task AsegurarPuertasBaseAsync(string codigoAeropuerto)
        => _unitOfWork.EjecutarEnTransaccionAsync(async () =>
        {
            var aeropuerto = await _repository.ObtenerAeropuertoPorCodigoAsync(codigoAeropuerto.Trim());
            if (aeropuerto is null)
                return; // el aeropuerto base aún no existe: nada que sembrar

            var terminales = await _repository.ObtenerTerminalesAsync();
            if (terminales.Any(t => t.AeropuertoId == aeropuerto.Id))
                return; // ya sembrado: idempotente

            // Terminal A y B del AILA, con 8 puertas cada una.
            foreach (var (codigo, nombre) in new[] { ("A", "Terminal A"), ("B", "Terminal B") })
            {
                var terminal = Terminal.Crear(codigo, nombre, aeropuerto.Id);
                await _repository.GuardarTerminalAsync(terminal);
                for (var i = 1; i <= 8; i++)
                    await _repository.GuardarPuertaAsync(Puerta.Crear($"{codigo}{i}", terminal.Id));
            }

            // Rampa abierta: posición remota en plataforma, sin terminal.
            await _repository.GuardarPuertaAsync(Puerta.Crear("Rampa abierta", null));

            await _auditoria.RegistrarAsync("Catalogo", "SembrarPuertas", "Exitoso",
                $"Terminales y puertas base creadas para {aeropuerto.Codigo}.");
        });

    private async Task<Terminal> ObtenerTerminalRequeridaAsync(Guid id)
        => await _repository.ObtenerTerminalPorIdAsync(id)
           ?? throw new InvalidOperationException($"No se encontró la terminal con id {id}.");

    private async Task<Puerta> ObtenerPuertaRequeridaAsync(Guid id)
        => await _repository.ObtenerPuertaPorIdAsync(id)
           ?? throw new InvalidOperationException($"No se encontró la puerta con id {id}.");

    private static TerminalDto MapearTerminal(Terminal t)
        => new(t.Id, t.Codigo, t.Nombre, t.AeropuertoId, t.Activa);

    private async Task<PuertaDto> MapearPuertaAsync(Puerta puerta)
    {
        var terminal = puerta.TerminalId is { } tid ? await _repository.ObtenerTerminalPorIdAsync(tid) : null;
        return new PuertaDto(puerta.Id, puerta.Codigo, puerta.TerminalId, terminal?.Nombre, puerta.EsRampa, puerta.Activa);
    }

    private async Task<Aerolinea> ObtenerAerolineaRequeridaAsync(Guid id)
        => await _repository.ObtenerAerolineaPorIdAsync(id)
           ?? throw new InvalidOperationException($"No se encontró la aerolínea con id {id}.");

    private async Task<Aeropuerto> ObtenerAeropuertoRequeridoAsync(Guid id)
        => await _repository.ObtenerAeropuertoPorIdAsync(id)
           ?? throw new InvalidOperationException($"No se encontró el aeropuerto con id {id}.");

    private static AerolineaDto Mapear(Aerolinea aerolinea)
        => new(aerolinea.Id, aerolinea.Codigo, aerolinea.Nombre, aerolinea.Activa);

    private static AeropuertoDto Mapear(Aeropuerto aeropuerto)
        => new(aeropuerto.Id, aeropuerto.Codigo, aeropuerto.Nombre, aeropuerto.Pais, aeropuerto.Activo);
}
