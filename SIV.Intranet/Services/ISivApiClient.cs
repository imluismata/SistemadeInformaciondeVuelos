using SIV.Intranet.Models;

namespace SIV.Intranet.Services;

/// <summary>
/// Contratos del cliente de la API, separados por área funcional para que
/// cada controlador dependa únicamente de lo que usa (Interface Segregation).
/// </summary>

public interface IAutenticacionApi
{
    Task<ResultadoLogin?> LoginAsync(string email, string password);
}

public interface ICatalogoApi
{
    Task<IReadOnlyList<AerolineaApi>> ObtenerAerolineasAsync();
    Task<IReadOnlyList<AeropuertoApi>> ObtenerAeropuertosAsync();

    Task<ResultadoOperacion> GuardarAerolineaAsync(AerolineaViewModel aerolinea);
    Task<ResultadoOperacion> DesactivarAerolineaAsync(Guid id);
    Task<ResultadoOperacion> ReactivarAerolineaAsync(Guid id);

    Task<ResultadoOperacion> GuardarAeropuertoAsync(AeropuertoViewModel aeropuerto);
    Task<ResultadoOperacion> DesactivarAeropuertoAsync(Guid id);
    Task<ResultadoOperacion> ReactivarAeropuertoAsync(Guid id);
}

public interface IAuditoriaApi
{
    /// <summary>Consulta el log. Solo lectura: no hay escritura por diseño (RNF-SEG-04).</summary>
    Task<IReadOnlyList<AuditoriaApi>> ConsultarAsync(string? modulo, string? accion, DateTime? desde, DateTime? hasta);
}

public interface IVuelosApi
{
    Task<IReadOnlyList<VueloApi>> ObtenerTodosAsync();
    Task<IReadOnlyList<VueloApi>> ConsultarAsync(VuelosFiltroViewModel filtro);
    Task<VueloDetalleApi?> ObtenerPorIdAsync(Guid id);
    Task<ResultadoOperacion> RegistrarAsync(RegistrarVueloViewModel vuelo);
    Task<ResultadoOperacion> ActualizarAsync(EditarVueloViewModel vuelo);
    Task<ResultadoOperacion> CambiarEstadoAsync(Guid id, string estadoNuevo);
    Task<ResultadoOperacion> RegistrarCambioOperativoAsync(Guid id, CambioOperativoViewModel cambio);
}

public interface IUsuariosApi
{
    Task<IReadOnlyList<UsuarioApi>> ObtenerTodosAsync();
    Task<ResultadoOperacion> CrearInternoAsync(CrearUsuarioViewModel usuario);
}

public interface IReportesApi
{
    Task<ReporteOperacionApi?> OperacionAsync(DateTime? desde, DateTime? hasta);
    Task<ReporteCambiosApi?> CambiosAsync(DateTime? desde, DateTime? hasta);
    Task<ReporteSeguimientoApi?> SeguimientoAsync();
    Task<byte[]> OperacionCsvAsync(DateTime? desde, DateTime? hasta);
    Task<byte[]> CambiosCsvAsync(DateTime? desde, DateTime? hasta);
    Task<byte[]> SeguimientoCsvAsync();
}

public interface IActividadApi
{
    Task<IReadOnlyList<SeguimientoRegistroApi>> SeguimientosAsync();
    Task<IReadOnlyList<NotificacionRegistroApi>> NotificacionesAsync();
}
