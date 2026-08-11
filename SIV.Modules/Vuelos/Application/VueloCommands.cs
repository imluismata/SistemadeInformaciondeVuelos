using SIV.Shared.Enums;

namespace SIV.Modules.Vuelos.Application;

public sealed record RegistrarVueloCommand(
    string Numero,
    Guid AerolineaId,
    Guid AeropuertoOrigenId,
    Guid AeropuertoDestinoId,
    DateTime HorarioSalida,
    DateTime HorarioLlegada,
    Guid? PuertaId = null);

public sealed record ActualizarEstadoVueloCommand(EstadoVuelo EstadoNuevo);

public sealed record RegistrarCambioOperativoCommand(
    TipoCambioOperativo Tipo,
    string Motivo,
    TimeSpan? Duracion = null,
    Guid? NuevaPuertaId = null);

public sealed record ActualizarDatosVueloCommand(
    Guid AerolineaId,
    Guid AeropuertoOrigenId,
    Guid AeropuertoDestinoId,
    DateTime HorarioSalida,
    DateTime HorarioLlegada,
    Guid? PuertaId,
    string Motivo);

