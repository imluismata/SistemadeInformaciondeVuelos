using SIV.Modules.Catalogo.Domain;
using SIV.Shared.Contracts;
using SIV.Shared.DTOs;

namespace SIV.Modules.Catalogo.Application;

internal sealed class CatalogoService : ICatalogoService
{
    private readonly ICatalogoRepository _repository;
    private readonly IAuditoriaService _auditoria;

    public CatalogoService(ICatalogoRepository repository, IAuditoriaService auditoria)
    {
        _repository = repository;
        _auditoria = auditoria;
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

    public async Task<AerolineaDto> RegistrarAerolineaAsync(RegistrarAerolineaCommand command)
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
    }

    public async Task<AerolineaDto> ActualizarAerolineaAsync(Guid id, ActualizarAerolineaCommand command)
    {
        var aerolinea = await ObtenerAerolineaRequeridaAsync(id);
        aerolinea.Actualizar(command.Codigo, command.Nombre);
        await _repository.GuardarAerolineaAsync(aerolinea);
        await _auditoria.RegistrarAsync("Catalogo", "ActualizarAerolinea", "Exitoso", $"Aerolínea {aerolinea.Codigo} actualizada.");
        return Mapear(aerolinea);
    }

    public async Task DesactivarAerolineaAsync(Guid id)
    {
        var aerolinea = await ObtenerAerolineaRequeridaAsync(id);
        aerolinea.Desactivar();
        await _repository.GuardarAerolineaAsync(aerolinea);
    }

    public async Task<AeropuertoDto> RegistrarAeropuertoAsync(RegistrarAeropuertoCommand command)
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
    }

    public async Task<AeropuertoDto> ActualizarAeropuertoAsync(Guid id, ActualizarAeropuertoCommand command)
    {
        var aeropuerto = await ObtenerAeropuertoRequeridoAsync(id);
        aeropuerto.Actualizar(command.Codigo, command.Nombre, command.Pais);
        await _repository.GuardarAeropuertoAsync(aeropuerto);
        return Mapear(aeropuerto);
    }

    public async Task DesactivarAeropuertoAsync(Guid id)
    {
        var aeropuerto = await ObtenerAeropuertoRequeridoAsync(id);
        aeropuerto.Desactivar();
        await _repository.GuardarAeropuertoAsync(aeropuerto);
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
