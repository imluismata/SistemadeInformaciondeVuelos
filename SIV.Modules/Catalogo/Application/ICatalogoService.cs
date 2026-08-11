using SIV.Shared.DTOs;

namespace SIV.Modules.Catalogo.Application;

public interface ICatalogoService
{
    Task<IReadOnlyList<AerolineaDto>> ObtenerAerolineasAsync();
    Task<IReadOnlyList<AeropuertoDto>> ObtenerAeropuertosAsync();
    Task<AerolineaDto> RegistrarAerolineaAsync(RegistrarAerolineaCommand command);
    Task<AerolineaDto> ActualizarAerolineaAsync(Guid id, ActualizarAerolineaCommand command);
    Task DesactivarAerolineaAsync(Guid id);
    Task ReactivarAerolineaAsync(Guid id);
    Task<AeropuertoDto> RegistrarAeropuertoAsync(RegistrarAeropuertoCommand command);
    Task<AeropuertoDto> ActualizarAeropuertoAsync(Guid id, ActualizarAeropuertoCommand command);
    Task DesactivarAeropuertoAsync(Guid id);
    Task ReactivarAeropuertoAsync(Guid id);

    Task<IReadOnlyList<TerminalDto>> ObtenerTerminalesAsync();
    Task<TerminalDto> RegistrarTerminalAsync(RegistrarTerminalCommand command);
    Task<TerminalDto> ActualizarTerminalAsync(Guid id, ActualizarTerminalCommand command);
    Task DesactivarTerminalAsync(Guid id);
    Task ReactivarTerminalAsync(Guid id);

    Task<IReadOnlyList<PuertaDto>> ObtenerPuertasAsync();
    Task<PuertaDto> RegistrarPuertaAsync(RegistrarPuertaCommand command);
    Task<PuertaDto> ActualizarPuertaAsync(Guid id, ActualizarPuertaCommand command);
    Task DesactivarPuertaAsync(Guid id);
    Task ReactivarPuertaAsync(Guid id);

    /// <summary>Crea las terminales y puertas base de un aeropuerto (por código) si aún no existen. Idempotente.</summary>
    Task AsegurarPuertasBaseAsync(string codigoAeropuerto);
}
