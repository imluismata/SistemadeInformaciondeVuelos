namespace SIV.Modules.Notificaciones.Application.Dtos;

// Registro del sistema de notificaciones (CU-NOT-04 / RNF-TRZ-03): qué se notificó,
// a quién (correo del destinatario), sobre qué vuelo y cuándo.
public record RegistroNotificacionDto(
    Guid Id,
    Guid UsuarioId,
    string Usuario,
    Guid VueloId,
    string Mensaje,
    string Estado,
    DateTime GeneradaEn,
    DateTime? LeidaEn
);
