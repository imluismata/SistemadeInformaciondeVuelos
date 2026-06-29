namespace SIV.Modules.Notificaciones.Application;

public record NotificacionDto(
    Guid Id,
    Guid VueloId,
    string Mensaje,
    string Estado,
    DateTime GeneradaEn,
    DateTime? LeidaEn
);
