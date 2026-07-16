namespace SIV.Modules.Seguimiento.Application.Dtos;

public record SeguimientoDto(
    Guid Id,
    Guid UsuarioId,
    Guid VueloId,
    string Estado,
    DateTime CreadoEn,
    DateTime? CanceladoEn
);
