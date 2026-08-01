namespace SIV.Intranet.Models;

/// <summary>
/// Modelos propios de la intranet para (de)serializar el JSON de la API.
/// Son independientes de los DTOs internos del backend: la intranet solo
/// conoce el contrato HTTP, nunca los proyectos del dominio.
/// </summary>

// Respuesta de POST /api/auth/login
// Respuesta de POST /api/auth/login: el token y los datos del usuario.
public sealed record ResultadoLogin(string Token, UsuarioLogin Usuario);

public sealed record UsuarioLogin(Guid Id, string Nombre, string Email, string Rol);

// Elemento de GET /api/vuelos
public sealed record VueloApi(
    Guid Id,
    string Numero,
    Guid AerolineaId,
    Guid AeropuertoOrigenId,
    Guid AeropuertoDestinoId,
    DateTime HorarioSalida,
    DateTime HorarioLlegada,
    string? Puerta,
    string EstadoActual);

// GET /api/vuelos/{id} — incluye historial y cambios operativos
public sealed record VueloDetalleApi(
    Guid Id,
    string Numero,
    Guid AerolineaId,
    Guid AeropuertoOrigenId,
    Guid AeropuertoDestinoId,
    DateTime HorarioSalida,
    DateTime HorarioLlegada,
    string? Puerta,
    string EstadoActual,
    IReadOnlyList<HistorialEstadoApi> HistorialEstados,
    IReadOnlyList<CambioOperativoApi> CambiosOperativos);

public sealed record HistorialEstadoApi(
    Guid Id,
    string EstadoAnterior,
    string EstadoNuevo,
    DateTime OcurridoEn);

public sealed record CambioOperativoApi(
    Guid Id,
    string Tipo,
    string Motivo,
    string? ValorAnterior,
    string? ValorNuevo,
    DateTime RegistradoEn);

// GET /api/catalogo/aerolineas y /aeropuertos
public sealed record AerolineaApi(Guid Id, string Codigo, string Nombre, bool Activa);

public sealed record AeropuertoApi(Guid Id, string Codigo, string Nombre, string Pais, bool Activo);
