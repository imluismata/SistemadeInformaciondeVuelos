namespace SIV.Modules.Notificaciones.Application.Dtos;

public record NotificacionDto(
    Guid Id,
    Guid VueloId,
    string Mensaje,
    string Estado,
    DateTime GeneradaEn,
    DateTime? LeidaEn
);
