namespace SIV.Shared.DTOs;

/// <summary>
/// Una fila del archivo tal cual se leyó/editó (todo texto, sin convertir). El
/// <see cref="Linea"/> es el número de línea/fila original, para referencia del usuario.
/// Es también el payload que la intranet reenvía tras editar la previsualización.
/// </summary>
public sealed record FilaVueloImportacion(
    int Linea,
    string? Numero,
    string? Aerolinea,
    string? Origen,
    string? Destino,
    string? Salida,
    string? Llegada,
    string? Puerta);

/// <summary>
/// Resultado de validar/importar una fila: la fila (para mostrarla y editarla), si está
/// apta, y los errores si no. En la importación real, <see cref="Errores"/> también recoge
/// los choques detectados al guardar (duplicado, solape de puerta contra otra fila, etc.).
/// </summary>
public sealed record FilaImportacionResultado(
    FilaVueloImportacion Fila,
    bool Valido,
    IReadOnlyList<string> Errores);

/// <summary>
/// Resumen de una previsualización o importación de vuelos.
/// </summary>
public sealed record ImportacionVuelosResultado(
    int Total,
    int Validas,
    int Importadas,
    IReadOnlyList<FilaImportacionResultado> Filas);
