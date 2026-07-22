namespace SIV.Intranet.Models;

/// <summary>
/// Modelos propios de la intranet para (de)serializar el JSON de la API.
/// Son independientes de los DTOs internos del backend: la intranet solo
/// conoce el contrato HTTP, nunca los proyectos del dominio.
/// </summary>

// Respuesta de POST /api/auth/login
public sealed record ResultadoLogin(string Token, string Nombre, string Rol);

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
