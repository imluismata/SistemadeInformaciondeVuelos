namespace SIV.Intranet.Services;

/// <summary>
/// Resultado de una operación de escritura contra la API. Permite trasladar
/// a la vista el mensaje de regla de negocio que devuelve el backend
/// (por ejemplo, una transición de estado inválida) sin usar excepciones
/// para el flujo normal.
/// </summary>
public sealed record ResultadoOperacion(bool Exito, string? Error = null)
{
    public static ResultadoOperacion Ok() => new(true);
    public static ResultadoOperacion Fallo(string error) => new(false, error);
}
