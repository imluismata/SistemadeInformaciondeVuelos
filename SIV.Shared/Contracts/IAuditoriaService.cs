using SIV.Shared.DTOs;

namespace SIV.Shared.Contracts;

public interface IAuditoriaService
{
    Task RegistrarAsync(string modulo, string accion, string resultado, string? detalle = null);
    Task<IReadOnlyList<AuditoriaDto>> ConsultarAsync(string? modulo, string? accion, DateTime? desde, DateTime? hasta);
}
