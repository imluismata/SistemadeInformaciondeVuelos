namespace SIV.Shared.DTOs;

// Un conteo simple (clave → cantidad), reutilizable en los distintos reportes.
public sealed record ConteoDto(string Clave, int Cantidad);

// CU-REP-01: Reporte de operación — cuántos vuelos hay por estado en el período.
public sealed record ReporteOperacionDto(
    DateTime? Desde,
    DateTime? Hasta,
    int Total,
    IReadOnlyList<ConteoDto> PorEstado);

// Una línea del detalle del reporte de cambios.
public sealed record CambioReporteDto(
    DateTime RegistradoEn,
    string Vuelo,
    string Tipo,
    string Motivo);

// CU-REP-02: Reporte de cambios operativos — conteo por tipo y detalle en el período.
public sealed record ReporteCambiosDto(
    DateTime? Desde,
    DateTime? Hasta,
    int Total,
    IReadOnlyList<ConteoDto> PorTipo,
    IReadOnlyList<CambioReporteDto> Detalle);

// Un vuelo con su número de seguidores activos.
public sealed record VueloSeguidoDto(string Vuelo, int Seguidores);

// CU-REP-03: Reporte de seguimiento — vuelos más seguidos y total de seguimientos activos.
public sealed record ReporteSeguimientoDto(
    int TotalSeguimientosActivos,
    IReadOnlyList<VueloSeguidoDto> VuelosMasSeguidos);
