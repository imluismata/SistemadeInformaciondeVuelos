using SIV.Modules.Auditoria.Domain;

namespace SIV.Modules.Auditoria.Application;

internal interface IAuditoriaRepository
{
    Task GuardarAsync(RegistroAuditoria registro);
    Task<IReadOnlyList<RegistroAuditoria>> ConsultarAsync(string? modulo, string? accion, DateTime? desde, DateTime? hasta);
}
