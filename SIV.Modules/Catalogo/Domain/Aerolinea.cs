namespace SIV.Modules.Catalogo.Domain;

internal sealed class Aerolinea
{
    private Aerolinea()
    {
    }

    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public bool Activa { get; private set; }

    public void Actualizar(string codigo, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la aerolínea es obligatorio.", nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre de la aerolínea es obligatorio.", nameof(nombre));
        }

        Codigo = codigo.Trim();
        Nombre = nombre.Trim();
    }

    public void Desactivar() => Activa = false;

    public static Aerolinea Crear(string codigo, string nombre)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código de la aerolínea es obligatorio.", nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre de la aerolínea es obligatorio.", nameof(nombre));
        }

        return new Aerolinea
        {
            Id = Guid.NewGuid(),
            Codigo = codigo.Trim(),
            Nombre = nombre.Trim(),
            Activa = true
        };
    }
}
