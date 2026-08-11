using SIV.Shared.DTOs;

namespace SIV.Modules.Vuelos.Application;

/// <summary>
/// Puerto de lectura de archivos de importación de vuelos (CSV o Excel). El módulo de
/// Vuelos define el contrato; la implementación concreta (con la librería de Excel) vive
/// en SIV.Infrastructure, para no acoplar el dominio a formatos de archivo (DIP).
/// </summary>
public interface ILectorVuelosImportados
{
    /// <summary>
    /// Convierte el contenido del archivo en filas crudas (texto). Lanza
    /// <see cref="FormatoArchivoNoSoportadoException"/> si la extensión no es .csv/.xlsx, o
    /// <see cref="ArchivoIlegibleException"/> si no se puede leer.
    /// </summary>
    IReadOnlyList<FilaVueloImportacion> Leer(Stream contenido, string nombreArchivo);
}

// Ambas se tratan como entrada inválida del cliente (heredan de ArgumentException → HTTP 400).

/// <summary>El archivo no es un formato soportado (solo .csv y .xlsx).</summary>
public sealed class FormatoArchivoNoSoportadoException(string mensaje) : ArgumentException(mensaje);

/// <summary>El archivo tiene el formato correcto pero no se pudo leer (corrupto/ilegible).</summary>
public sealed class ArchivoIlegibleException(string mensaje) : ArgumentException(mensaje);
