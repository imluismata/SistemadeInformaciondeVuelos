namespace SIV.Intranet.Models;

/// <summary>
/// Vocabulario de estados que la intranet ofrece en los formularios.
/// Intencionalmente NO contiene la máquina de transiciones: qué cambio es
/// válido lo decide el dominio en la API, para no duplicar reglas de negocio.
/// </summary>
public static class EstadosVuelo
{
    public static IReadOnlyList<string> Todos { get; } =
    [
        "Programado",
        "Retrasado",
        "Embarcando",
        "EnVuelo",
        "Aterrizado",
        "Completado",
        "Cancelado"
    ];
}
