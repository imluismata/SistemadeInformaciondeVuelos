using SIV.Modules.Vuelos.Domain;

namespace SIV.Modules.Vuelos.Application;

public sealed record RegistrarVueloCommand(
    string Numero,
    Guid AerolineaId,
    Guid AeropuertoOrigenId,
    Guid AeropuertoDestinoId,
    DateTime HorarioSalida,
    DateTime HorarioLlegada,
    string? Puerta = null);

public sealed record ActualizarEstadoVueloCommand(EstadoVuelo EstadoNuevo);

public sealed record RegistrarCambioOperativoCommand(
    TipoCambioOperativo Tipo,
    string Motivo,
    TimeSpan? Duracion = null,
    string? NuevaPuerta = null);

public sealed record ActualizarDatosVueloCommand(
    Guid AerolineaId,
    Guid AeropuertoOrigenId,
    Guid AeropuertoDestinoId,
    DateTime HorarioSalida,
    DateTime HorarioLlegada,
    string? Puerta,
    string Motivo);

