namespace SIV.Intranet.Models;

// Registro de seguimiento tal como lo devuelve GET /api/seguimiento/todos (CU-SEG-04).
public sealed record SeguimientoRegistroApi(
    Guid Id, Guid UsuarioId, string Usuario, Guid VueloId, string Vuelo,
    string Estado, DateTime CreadoEn, DateTime? CanceladoEn);

// Registro de notificación tal como lo devuelve GET /api/notificaciones/registro (CU-NOT-04).
public sealed record NotificacionRegistroApi(
    Guid Id, Guid UsuarioId, string Usuario, Guid VueloId, string Mensaje,
    string Estado, DateTime GeneradaEn, DateTime? LeidaEn);

// Pantalla de actividad de usuarios (admin/auditor): seguimientos y notificaciones.
public sealed class ActividadViewModel
{
    public IReadOnlyList<SeguimientoRegistroApi> Seguimientos { get; set; } = [];
    public IReadOnlyList<NotificacionRegistroApi> Notificaciones { get; set; } = [];
}
