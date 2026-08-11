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

// Elemento de GET /api/vuelos. 'Puerta' es la descripción ya resuelta
// ("A5 · Terminal A"); 'PuertaId' es la referencia para preseleccionar en los
// formularios.
public sealed record VueloApi(
    Guid Id,
    string Numero,
    Guid AerolineaId,
    Guid AeropuertoOrigenId,
    Guid AeropuertoDestinoId,
    DateTime HorarioSalida,
    DateTime HorarioLlegada,
    Guid? PuertaId,
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
    Guid? PuertaId,
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

// Fila de importación (todos los campos como texto): se muestra y edita en la previsualización.
public sealed record FilaVueloApi(
    int Linea, string? Numero, string? Aerolinea, string? Origen, string? Destino,
    string? Salida, string? Llegada, string? Puerta);

// Resultado por fila: la fila + si está apta + errores.
public sealed record FilaImportacionApi(FilaVueloApi Fila, bool Valido, IReadOnlyList<string> Errores);

public sealed record ImportacionResultadoApi(int Total, int Validas, int Importadas, IReadOnlyList<FilaImportacionApi> Filas);

// Envoltura de la intranet: éxito/error del llamado + el resultado si hubo.
public sealed record ImportacionRespuesta(bool Exito, string? Error, ImportacionResultadoApi? Resultado);

// GET /api/catalogo/terminales y /puertas
public sealed record TerminalApi(Guid Id, string Codigo, string Nombre, Guid AeropuertoId, bool Activa);

// 'TerminalNombre' viene resuelto; es nulo cuando la puerta es una rampa abierta
// (EsRampa == true).
public sealed record PuertaApi(Guid Id, string Codigo, Guid? TerminalId, string? TerminalNombre, bool EsRampa, bool Activa);
