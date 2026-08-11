namespace SIV.Modules.Catalogo.Domain;

/// <summary>
/// Terminal de un aeropuerto (p. ej. Terminal A y B del AILA). Agrupa puertas de
/// embarque. Sigue el mismo patrón que Aerolinea/Aeropuerto: entidad interna con
/// factory, actualización y borrado lógico (Activa).
/// </summary>
internal sealed class Terminal
{
    private Terminal() { }

    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public Guid AeropuertoId { get; private set; }
    public bool Activa { get; private set; }

    public void Actualizar(string codigo, string nombre)
    {
        Validar(codigo, nombre);
        Codigo = codigo.Trim();
        Nombre = nombre.Trim();
    }

    public void Desactivar() => Activa = false;

    public void Reactivar() => Activa = true;

    public static Terminal Crear(string codigo, string nombre, Guid aeropuertoId)
    {
        Validar(codigo, nombre);
        if (aeropuertoId == Guid.Empty)
            throw new ArgumentException("La terminal debe pertenecer a un aeropuerto.", nameof(aeropuertoId));

        return new Terminal
        {
            Id = Guid.NewGuid(),
            Codigo = codigo.Trim(),
            Nombre = nombre.Trim(),
            AeropuertoId = aeropuertoId,
            Activa = true
        };
    }

    private static void Validar(string codigo, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            throw new ArgumentException("El código de la terminal es obligatorio.", nameof(codigo));
        if (string.IsNullOrWhiteSpace(nombre))
            throw new ArgumentException("El nombre de la terminal es obligatorio.", nameof(nombre));
    }
}
