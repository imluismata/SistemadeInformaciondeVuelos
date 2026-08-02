using SIV.Shared.Enums;

namespace SIV.Modules.Vuelos.Domain;

internal sealed class HistorialEstado
{
    private HistorialEstado() { }

    public HistorialEstado(Guid id, Guid vueloId, EstadoVuelo estadoAnterior, EstadoVuelo estadoNuevo, DateTime ocurridoEn)
    {
        Id = id;
        VueloId = vueloId;
        EstadoAnterior = estadoAnterior;
        EstadoNuevo = estadoNuevo;
        OcurridoEn = ocurridoEn;
    }

    public Guid Id { get; private set; }
    public Guid VueloId { get; private set; }
    public EstadoVuelo EstadoAnterior { get; private set; }
    public EstadoVuelo EstadoNuevo { get; private set; }
    public DateTime OcurridoEn { get; private set; }
}
