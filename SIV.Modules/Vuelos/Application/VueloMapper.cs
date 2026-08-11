using SIV.Modules.Vuelos.Domain;
using SIV.Shared.DTOs;

namespace SIV.Modules.Vuelos.Application;

// Mapeo centralizado de la entidad Vuelo al VueloDto compartido, para que
// VueloService y VueloConsultaService no dupliquen la conversión (DRY).
internal static class VueloMapper
{
    public static VueloDto ADto(Vuelo vuelo) =>
        new(vuelo.Id,
            vuelo.Numero,
            vuelo.AerolineaId,
            vuelo.AeropuertoOrigenId,
            vuelo.AeropuertoDestinoId,
            vuelo.HorarioSalida,
            vuelo.HorarioLlegada,
            vuelo.PuertaId,
            vuelo.PuertaDescripcion,
            vuelo.EstadoActual.ToString(),
            vuelo.HistorialEstados
                .OrderBy(h => h.OcurridoEn)
                .Select(h => new HistorialEstadoDto(
                    h.Id, h.VueloId, h.EstadoAnterior.ToString(), h.EstadoNuevo.ToString(), h.OcurridoEn)).ToList(),
            vuelo.CambiosOperativos
                .OrderBy(c => c.RegistradoEn)
                .Select(c => new CambioOperativoDto(
                    c.Id, c.VueloId, c.Tipo.ToString(), c.Motivo, c.ValorAnterior, c.ValorNuevo, c.RegistradoEn)).ToList());
}
