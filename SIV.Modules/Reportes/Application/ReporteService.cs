using SIV.Shared.Contracts;
using SIV.Shared.DTOs;

namespace SIV.Modules.Reportes.Application;

/// <summary>
/// Implementación de <see cref="IReporteService"/>. Lee los vuelos a través del
/// contrato compartido <see cref="IVueloConsulta"/> y calcula las agregaciones.
/// No conoce las entidades internas de Vuelos: solo depende de la abstracción y
/// del VueloDto compartido (DA-02 / Dependency Inversion).
/// </summary>
internal sealed class ReporteService(IVueloConsulta vuelos, ISeguimientoConsulta seguimientos) : IReporteService
{
    public async Task<ReporteOperacionDto> GenerarOperacionAsync(DateTime? desde, DateTime? hasta)
    {
        // Vuelos cuyo horario de salida cae en el período.
        var lista = await vuelos.ObtenerParaReporteAsync(desde, hasta);

        var porEstado = lista
            .GroupBy(v => v.EstadoActual)
            .Select(g => new ConteoDto(g.Key, g.Count()))
            .OrderByDescending(c => c.Cantidad)
            .ToList();

        return new ReporteOperacionDto(desde, hasta, lista.Count, porEstado);
    }

    public async Task<ReporteCambiosDto> GenerarCambiosAsync(DateTime? desde, DateTime? hasta)
    {
        // Para cambios, el período aplica a la FECHA DEL CAMBIO (no al horario del
        // vuelo), así que se traen todos los vuelos y se filtran sus cambios.
        var lista = await vuelos.ObtenerParaReporteAsync(null, null);

        var cambios = lista
            .SelectMany(v => v.CambiosOperativos.Select(c => new { v.Numero, Cambio = c }))
            .Where(x => (!desde.HasValue || x.Cambio.RegistradoEn >= desde.Value)
                     && (!hasta.HasValue || x.Cambio.RegistradoEn <= hasta.Value))
            .OrderByDescending(x => x.Cambio.RegistradoEn)
            .ToList();

        var porTipo = cambios
            .GroupBy(x => x.Cambio.Tipo)
            .Select(g => new ConteoDto(g.Key, g.Count()))
            .OrderByDescending(c => c.Cantidad)
            .ToList();

        var detalle = cambios
            .Select(x => new CambioReporteDto(x.Cambio.RegistradoEn, x.Numero, x.Cambio.Tipo, x.Cambio.Motivo))
            .ToList();

        return new ReporteCambiosDto(desde, hasta, cambios.Count, porTipo, detalle);
    }

    public async Task<ReporteSeguimientoDto> GenerarSeguimientoAsync()
    {
        // Conteo de seguidores por vuelo (del módulo Seguimiento) + número de vuelo
        // (del módulo Vuelos), ambos por contratos de Shared (DA-02).
        var conteos = await seguimientos.ContarSeguidoresActivosAsync();
        var numeros = (await vuelos.ObtenerParaReporteAsync(null, null)).ToDictionary(v => v.Id, v => v.Numero);

        var vuelosMasSeguidos = conteos
            .Select(c => new VueloSeguidoDto(numeros.GetValueOrDefault(c.VueloId, "—"), c.Seguidores))
            .OrderByDescending(v => v.Seguidores)
            .ToList();

        var total = conteos.Sum(c => c.Seguidores);
        return new ReporteSeguimientoDto(total, vuelosMasSeguidos);
    }
}
