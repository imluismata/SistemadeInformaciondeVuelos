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
