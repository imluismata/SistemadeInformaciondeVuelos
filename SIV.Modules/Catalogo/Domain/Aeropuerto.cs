namespace SIV.Modules.Catalogo.Domain;

public sealed class Aeropuerto
{
    private Aeropuerto()
    {
    }

    public Guid Id { get; private set; }
    public string Codigo { get; private set; } = string.Empty;
    public string Nombre { get; private set; } = string.Empty;
    public string Pais { get; private set; } = string.Empty;
    public bool Activo { get; private set; }

    public void Actualizar(string codigo, string nombre, string pais)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código del aeropuerto es obligatorio.", nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del aeropuerto es obligatorio.", nameof(nombre));
        }

        if (string.IsNullOrWhiteSpace(pais))
        {
            throw new ArgumentException("El país del aeropuerto es obligatorio.", nameof(pais));
        }

        Codigo = codigo.Trim();
        Nombre = nombre.Trim();
        Pais = pais.Trim();
    }

    public void Desactivar() => Activo = false;

    public static Aeropuerto Registrar(string codigo, string nombre, string pais)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            throw new ArgumentException("El código del aeropuerto es obligatorio.", nameof(codigo));
        }

        if (string.IsNullOrWhiteSpace(nombre))
        {
            throw new ArgumentException("El nombre del aeropuerto es obligatorio.", nameof(nombre));
        }

        if (string.IsNullOrWhiteSpace(pais))
        {
            throw new ArgumentException("El país del aeropuerto es obligatorio.", nameof(pais));
        }

        return new Aeropuerto
        {
            Id = Guid.NewGuid(),
            Codigo = codigo.Trim(),
            Nombre = nombre.Trim(),
            Pais = pais.Trim(),
            Activo = true
        };
    }
}
