using SIV.Modules.Auditoria.Domain;
using SIV.Shared.Contracts;
using SIV.Shared.DTOs;

namespace SIV.Modules.Auditoria.Application;

internal sealed class AuditoriaService(IAuditoriaRepository repository, IUsuarioActual usuarioActual) : IAuditoriaService
{
    public async Task RegistrarAsync(string modulo, string accion, string resultado, string? detalle = null)
    {
        // El actor se resuelve aquí, no lo pasa cada servicio: AuditoriaService
        // pregunta quién ejecuta la petición. Así todos los módulos registran el
        // actor sin modificar sus llamadas a la auditoría.
        var registro = RegistroAuditoria.Crear(modulo, accion, resultado, usuarioActual.Descripcion, detalle);
        await repository.GuardarAsync(registro);
    }

    public async Task<IReadOnlyList<AuditoriaDto>> ConsultarAsync(string? modulo, string? accion, DateTime? desde, DateTime? hasta)
    {
        var registros = await repository.ConsultarAsync(modulo, accion, desde, hasta);
        return registros.Select(r => new AuditoriaDto(r.Id, r.Modulo, r.Accion, r.Detalle, r.Resultado, r.Actor, r.FechaHora)).ToList();
    }
}
