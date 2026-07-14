using SIV.Shared.DTOs;

namespace SIV.Modules.Catalogo.Application;

public interface ICatalogoService
{
    Task<IReadOnlyList<AerolineaDto>> ObtenerAerolineasAsync();
    Task<IReadOnlyList<AeropuertoDto>> ObtenerAeropuertosAsync();
    Task<AerolineaDto> RegistrarAerolineaAsync(RegistrarAerolineaCommand command);
    Task<AerolineaDto> ActualizarAerolineaAsync(Guid id, ActualizarAerolineaCommand command);
    Task DesactivarAerolineaAsync(Guid id);
    Task<AeropuertoDto> RegistrarAeropuertoAsync(RegistrarAeropuertoCommand command);
    Task<AeropuertoDto> ActualizarAeropuertoAsync(Guid id, ActualizarAeropuertoCommand command);
    Task DesactivarAeropuertoAsync(Guid id);
}
