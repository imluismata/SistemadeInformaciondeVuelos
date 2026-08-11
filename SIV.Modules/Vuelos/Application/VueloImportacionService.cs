using System.Globalization;
using SIV.Shared.Contracts;
using SIV.Shared.DTOs;

namespace SIV.Modules.Vuelos.Application;

public interface IVueloImportacionService
{
    /// <summary>Lee el archivo y valida sus filas sin guardar; devuelve cada fila (para editarla) con su estado.</summary>
    Task<ImportacionVuelosResultado> PrevisualizarArchivoAsync(Stream contenido, string nombreArchivo);

    /// <summary>Valida un conjunto de filas ya editadas (sin guardar), para revalidar tras corregir.</summary>
    Task<ImportacionVuelosResultado> ValidarFilasAsync(IEnumerable<FilaVueloImportacion> filas);

    /// <summary>Importa las filas válidas; reporta cuáles se guardaron y cuáles se omitieron y por qué.</summary>
    Task<ImportacionVuelosResultado> ImportarFilasAsync(IEnumerable<FilaVueloImportacion> filas);
}

/// <summary>
/// Importación masiva de vuelos desde CSV/Excel. Orquesta: leer el archivo (por el puerto
/// <see cref="ILectorVuelosImportados"/>), resolver códigos a identificadores contra el
/// catálogo, y validar/registrar cada fila reutilizando las reglas de <see cref="IVueloService"/>
/// para que la previsualización y la importación coincidan exactamente.
/// </summary>
internal sealed class VueloImportacionService(
    ILectorVuelosImportados lector,
    ICatalogoConsulta catalogo,
    IVueloService vuelos) : IVueloImportacionService
{
    // Formatos de fecha aceptados en el archivo (hora local de pizarra). El primero es el
    // recomendado; los demás cubren variantes comunes al exportar desde Excel.
    private static readonly string[] FormatosFecha =
        ["yyyy-MM-dd HH:mm", "yyyy-MM-dd'T'HH:mm", "yyyy-MM-dd HH:mm:ss", "dd/MM/yyyy HH:mm"];

    public Task<ImportacionVuelosResultado> PrevisualizarArchivoAsync(Stream contenido, string nombreArchivo)
        => ValidarFilasAsync(lector.Leer(contenido, nombreArchivo));

    public async Task<ImportacionVuelosResultado> ValidarFilasAsync(IEnumerable<FilaVueloImportacion> filas)
    {
        var resultados = new List<FilaImportacionResultado>();

        foreach (var fila in filas)
        {
            var (comando, errores) = await ConstruirComandoAsync(fila);
            if (comando is not null)
                errores.AddRange(await vuelos.ValidarRegistroAsync(comando));

            resultados.Add(new FilaImportacionResultado(fila, errores.Count == 0, errores));
        }

        return Resumir(resultados, importadas: 0);
    }

    public async Task<ImportacionVuelosResultado> ImportarFilasAsync(IEnumerable<FilaVueloImportacion> filas)
    {
        var resultados = new List<FilaImportacionResultado>();
        var importadas = 0;

        foreach (var fila in filas)
        {
            var (comando, errores) = await ConstruirComandoAsync(fila);
            if (comando is null)
            {
                resultados.Add(new FilaImportacionResultado(fila, false, errores));
                continue;
            }

            try
            {
                // Se registra por el flujo normal: valida (unicidad, catálogo, solape de puerta)
                // y persiste en su propia transacción. Así una fila que choque contra otra ya
                // importada del mismo lote también se detecta y se reporta.
                await vuelos.RegistrarAsync(comando);
                importadas++;
                resultados.Add(new FilaImportacionResultado(fila, true, []));
            }
            catch (Exception ex)
            {
                resultados.Add(new FilaImportacionResultado(fila, false, [ex.Message]));
            }
        }

        return Resumir(resultados, importadas);
    }

    private static ImportacionVuelosResultado Resumir(List<FilaImportacionResultado> filas, int importadas)
        => new(filas.Count, filas.Count(f => f.Valido), importadas, filas);

    // Convierte una fila cruda en un comando de registro, resolviendo códigos y fechas.
    // Devuelve (null, errores) si algún campo no se puede interpretar o resolver.
    private async Task<(RegistrarVueloCommand? Comando, List<string> Errores)> ConstruirComandoAsync(FilaVueloImportacion fila)
    {
        var errores = new List<string>();

        if (string.IsNullOrWhiteSpace(fila.Numero))
            errores.Add("El número de vuelo es obligatorio.");

        var aerolineaId = await ResolverAsync(fila.Aerolinea, "aerolínea",
            c => catalogo.ObtenerAerolineaIdPorCodigoAsync(c), errores);
        var origenId = await ResolverAsync(fila.Origen, "aeropuerto de origen",
            c => catalogo.ObtenerAeropuertoIdPorCodigoAsync(c), errores);
        var destinoId = await ResolverAsync(fila.Destino, "aeropuerto de destino",
            c => catalogo.ObtenerAeropuertoIdPorCodigoAsync(c), errores);

        var salida = ParsearFecha(fila.Salida, "La salida", errores);
        var llegada = ParsearFecha(fila.Llegada, "La llegada", errores);

        Guid? puertaId = null;
        if (!string.IsNullOrWhiteSpace(fila.Puerta))
        {
            var puerta = await catalogo.ObtenerPuertaPorCodigoAsync(fila.Puerta.Trim());
            if (puerta is null)
                errores.Add($"La puerta '{fila.Puerta.Trim()}' no existe en el catálogo.");
            else
                puertaId = puerta.Id;
        }

        if (errores.Count > 0)
            return (null, errores);

        var comando = new RegistrarVueloCommand(
            fila.Numero!.Trim(), aerolineaId!.Value, origenId!.Value, destinoId!.Value,
            salida!.Value, llegada!.Value, puertaId);

        return (comando, errores);
    }

    // Resuelve un código a Id; agrega un error si está vacío o no existe. Devuelve null en ese caso.
    private static async Task<Guid?> ResolverAsync(
        string? codigo, string nombre, Func<string, Task<Guid?>> resolver, List<string> errores)
    {
        if (string.IsNullOrWhiteSpace(codigo))
        {
            errores.Add($"El código de {nombre} es obligatorio.");
            return null;
        }

        var id = await resolver(codigo.Trim());
        if (id is null)
            errores.Add($"El código de {nombre} '{codigo.Trim()}' no existe en el catálogo.");

        return id;
    }

    // Interpreta la fecha con los formatos aceptados; agrega error si no es válida.
    private static DateTime? ParsearFecha(string? valor, string nombre, List<string> errores)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            errores.Add($"{nombre} es obligatoria.");
            return null;
        }

        if (DateTime.TryParseExact(valor.Trim(), FormatosFecha, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var fecha)
            || DateTime.TryParse(valor.Trim(), CultureInfo.InvariantCulture, DateTimeStyles.None, out fecha))
            return fecha;

        errores.Add($"{nombre} tiene un formato inválido (usa 2026-08-20 10:00).");
        return null;
    }
}
