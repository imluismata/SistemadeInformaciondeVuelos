using ClosedXML.Excel;
using SIV.Modules.Vuelos.Application;
using SIV.Shared.DTOs;

namespace SIV.Infrastructure.Importacion;

/// <summary>
/// Implementación del puerto <see cref="ILectorVuelosImportados"/>: lee un archivo .csv
/// (parser nativo) o .xlsx (ClosedXML) a filas crudas. Solo se ocupa del <b>formato</b> del
/// archivo; no valida ni convierte los datos —de eso se encarga el módulo de Vuelos—.
/// Vive en Infraestructura para que la librería de Excel no toque el dominio (DIP).
/// </summary>
internal sealed class LectorVuelosImportados : ILectorVuelosImportados
{
    // Orden esperado de columnas. La primera fila puede ser un encabezado (se detecta y omite).
    private const int ColNumero = 0, ColAerolinea = 1, ColOrigen = 2, ColDestino = 3,
                      ColSalida = 4, ColLlegada = 5, ColPuerta = 6;

    public IReadOnlyList<FilaVueloImportacion> Leer(Stream contenido, string nombreArchivo)
    {
        var extension = Path.GetExtension(nombreArchivo).ToLowerInvariant();
        return extension switch
        {
            ".csv" => LeerCsv(contenido),
            ".xlsx" => LeerExcel(contenido),
            _ => throw new FormatoArchivoNoSoportadoException(
                "Formato no soportado. Usa un archivo .csv o .xlsx.")
        };
    }

    private static IReadOnlyList<FilaVueloImportacion> LeerCsv(Stream contenido)
    {
        var filas = new List<FilaVueloImportacion>();
        using var lector = new StreamReader(contenido);

        string? linea;
        var numeroLinea = 0;
        while ((linea = lector.ReadLine()) is not null)
        {
            numeroLinea++;
            if (string.IsNullOrWhiteSpace(linea))
                continue;

            var campos = DividirCsv(linea);

            // La primera fila con datos, si parece encabezado, se omite.
            if (numeroLinea == 1 && EsEncabezado(campos))
                continue;

            filas.Add(ADto(numeroLinea, campos));
        }

        return filas;
    }

    private static IReadOnlyList<FilaVueloImportacion> LeerExcel(Stream contenido)
    {
        var filas = new List<FilaVueloImportacion>();

        try
        {
            using var libro = new XLWorkbook(contenido);
            var hoja = libro.Worksheets.First();

            foreach (var fila in hoja.RowsUsed())
            {
                var numeroFila = fila.RowNumber();
                var campos = Enumerable.Range(1, ColPuerta + 1)
                    .Select(c => Celda(fila.Cell(c)))
                    .ToArray();

                if (campos.All(string.IsNullOrWhiteSpace))
                    continue;

                if (numeroFila == 1 && EsEncabezado(campos))
                    continue;

                filas.Add(ADto(numeroFila, campos));
            }
        }
        catch (Exception ex) when (ex is not FormatoArchivoNoSoportadoException)
        {
            throw new ArchivoIlegibleException("No se pudo leer el archivo de Excel. Verifica que no esté dañado.");
        }

        return filas;
    }

    // Las celdas de fecha se normalizan a texto canónico para que el módulo las interprete igual
    // que las del CSV; el resto se toma como texto.
    private static string Celda(IXLCell celda)
        => celda.DataType == XLDataType.DateTime
            ? celda.GetDateTime().ToString("yyyy-MM-dd HH:mm")
            : celda.GetString().Trim();

    private static FilaVueloImportacion ADto(int linea, string[] c) => new(
        linea,
        Campo(c, ColNumero),
        Campo(c, ColAerolinea),
        Campo(c, ColOrigen),
        Campo(c, ColDestino),
        Campo(c, ColSalida),
        Campo(c, ColLlegada),
        Campo(c, ColPuerta));

    private static string? Campo(string[] campos, int indice)
    {
        if (indice >= campos.Length)
            return null;
        var valor = campos[indice].Trim();
        return valor.Length == 0 ? null : valor;
    }

    // ¿La fila es un encabezado? Si la primera celda es "numero"/"número", se asume que sí.
    private static bool EsEncabezado(string[] campos)
        => campos.Length > 0 && campos[0].Trim().ToLowerInvariant() is "numero" or "número";

    // Parser CSV mínimo que respeta comillas dobles y comas dentro de comillas.
    private static string[] DividirCsv(string linea)
    {
        var campos = new List<string>();
        var actual = new System.Text.StringBuilder();
        var entreComillas = false;

        for (var i = 0; i < linea.Length; i++)
        {
            var ch = linea[i];
            if (entreComillas)
            {
                if (ch == '"')
                {
                    // Comilla escapada ("") dentro de un campo entrecomillado.
                    if (i + 1 < linea.Length && linea[i + 1] == '"') { actual.Append('"'); i++; }
                    else entreComillas = false;
                }
                else actual.Append(ch);
            }
            else if (ch == '"') entreComillas = true;
            else if (ch == ',') { campos.Add(actual.ToString()); actual.Clear(); }
            else actual.Append(ch);
        }

        campos.Add(actual.ToString());
        return campos.ToArray();
    }
}
