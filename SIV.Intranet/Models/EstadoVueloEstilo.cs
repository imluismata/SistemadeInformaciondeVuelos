namespace SIV.Intranet.Models;

/// <summary>
/// Traduce el estado de un vuelo a las clases CSS del "status pill".
/// Centraliza la presentación del estado: agregar o cambiar un estilo
/// se hace en un solo lugar (OCP), sin tocar las vistas.
/// </summary>
public static class EstadoVueloEstilo
{
    private static readonly IReadOnlyDictionary<string, string> Clases =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["Programado"] = "bg-blue-50 text-blue-700",
            ["Retrasado"]  = "bg-amber-100 text-amber-700",
            ["Embarcando"] = "bg-indigo-100 text-indigo-700",
            ["EnVuelo"]    = "bg-sky-100 text-sky-700",
            ["Aterrizado"] = "bg-teal-100 text-teal-700",
            ["Completado"] = "bg-green-100 text-green-700",
            ["Cancelado"]  = "bg-red-100 text-red-700"
        };

    public static string ClasePill(string estado) =>
        Clases.TryGetValue(estado, out var clase) ? clase : "bg-gray-100 text-gray-700";
}
