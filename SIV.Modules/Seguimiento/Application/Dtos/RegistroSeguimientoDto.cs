namespace SIV.Modules.Seguimiento.Application.Dtos;

// Historial de seguimientos para admin/auditor (CU-SEG-04): quién sigue (correo)
// qué vuelo, en qué estado y desde cuándo.
public record RegistroSeguimientoDto(
    Guid Id,
    Guid UsuarioId,
    string Usuario,
    Guid VueloId,
    string Vuelo,
    string Estado,
    DateTime CreadoEn,
    DateTime? CanceladoEn
);
