namespace SIV.Modules.Auditoria.Domain;

public sealed class RegistroAuditoria
{
    private RegistroAuditoria() { }

    public Guid Id { get; private set; }
    public string Modulo { get; private set; } = string.Empty;
    public string Accion { get; private set; } = string.Empty;
    public string? Detalle { get; private set; }
    public string Resultado { get; private set; } = string.Empty;
    public DateTime FechaHora { get; private set; }

    public static RegistroAuditoria Crear(string modulo, string accion, string resultado, string? detalle = null)
    {
        if (string.IsNullOrWhiteSpace(modulo))
            throw new ArgumentException("El módulo es obligatorio.", nameof(modulo));

        if (string.IsNullOrWhiteSpace(accion))
            throw new ArgumentException("La acción es obligatoria.", nameof(accion));

        if (string.IsNullOrWhiteSpace(resultado))
            throw new ArgumentException("El resultado es obligatorio.", nameof(resultado));

        return new RegistroAuditoria
        {
            Id = Guid.NewGuid(),
            Modulo = modulo.Trim(),
            Accion = accion.Trim(),
            Detalle = detalle?.Trim(),
            Resultado = resultado.Trim(),
            FechaHora = DateTime.UtcNow
        };
    }
}
