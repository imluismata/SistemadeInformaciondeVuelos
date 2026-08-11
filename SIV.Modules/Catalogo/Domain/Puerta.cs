namespace SIV.Modules.Catalogo.Domain;

/// <summary>
/// Puerta de embarque (p. ej. A5). Pertenece a una terminal. Una puerta SIN terminal
/// (<see cref="TerminalId"/> nulo) representa una posición remota en plataforma:
/// la "Rampa abierta". Así el estado de un vuelo respecto a la puerta queda en un
/// único campo (PuertaId): en una puerta, en rampa, o sin asignar — sin banderas
/// redundantes que permitan estados contradictorios.
/// </summary>
internal sealed class Puerta
{
    private Puerta() { }

    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public Guid? TerminalId { get; private set; }
    public bool Activa { get; private set; }

    /// <summary>Verdadero cuando la puerta es una posición remota (sin terminal).</summary>
    public bool EsRampa => TerminalId is null;

    public void Actualizar(string codigo, Guid? terminalId)
    {
        Validar(codigo);
        Codigo = codigo.Trim();
        TerminalId = terminalId;
    }

    public void Desactivar() => Activa = false;

    public void Reactivar() => Activa = true;

    public static Puerta Crear(string codigo, Guid? terminalId)
    {
        Validar(codigo);
        return new Puerta
        {
            Id = Guid.NewGuid(),
            Codigo = codigo.Trim(),
            TerminalId = terminalId,
            Activa = true
        };
    }

    private static void Validar(string codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código de la puerta es obligatorio.", nameof(codigo));
    }
}
