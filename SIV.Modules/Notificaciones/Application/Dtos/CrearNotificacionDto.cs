namespace SIV.Modules.Notificaciones.Application.Dtos;

public record CrearNotificacionDto(Guid UsuarioId, Guid VueloId, string Mensaje);
