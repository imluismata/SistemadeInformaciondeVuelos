using SIV.Modules.Vuelos.Application;
using SIV.Shared.Contracts;
using SIV.Shared.DTOs;
using Xunit;

namespace SIV.Tests;

/// <summary>
/// Pruebas de la importación masiva de vuelos: la previsualización valida fila por fila sin
/// guardar, y la importación guarda las válidas y reporta las que se omiten (con su motivo).
/// Se usan dobles del lector de archivos, del catálogo y del servicio de vuelos, así que no
/// hacen falta archivos reales, base de datos ni API.
/// </summary>
public class ImportacionVuelosTests
{
    private static readonly Guid Qf = Guid.NewGuid();
    private static readonly Guid Sdq = Guid.NewGuid();
    private static readonly Guid Mia = Guid.NewGuid();

    [Fact]
    public async Task Previsualizar_MarcaValidaLaBuenaEInvalidaLaDeCodigoDesconocido()
    {
        var lector = new FakeLector(
            new FilaVueloImportacion(2, "QF801", "QF", "SDQ", "MIA", "2026-08-20 10:00", "2026-08-20 13:00", null),
            new FilaVueloImportacion(3, "QF802", "XX", "SDQ", "MIA", "2026-08-20 11:00", "2026-08-20 14:00", null));

        // El servicio de vuelos no encuentra reglas de negocio violadas (fila bien formada).
        var vuelos = new FakeVuelos(validar: _ => []);
        var service = new VueloImportacionService(lector, new FakeCatalogo(), vuelos);

        var resultado = await service.PrevisualizarArchivoAsync(Stream.Null, "vuelos.csv");

        Assert.Equal(2, resultado.Total);
        Assert.Equal(1, resultado.Validas);
        Assert.Equal(0, resultado.Importadas);              // previsualización: no guarda
        Assert.True(resultado.Filas.Single(f => f.Fila.Linea == 2).Valido);
        var mala = resultado.Filas.Single(f => f.Fila.Linea == 3);
        Assert.False(mala.Valido);
        Assert.Contains(mala.Errores, e => e.Contains("aerolínea") && e.Contains("XX"));
    }

    [Fact]
    public async Task Importar_GuardaLasValidasYOmiteLasQueChocan()
    {
        var filas = new[]
        {
            new FilaVueloImportacion(2, "QF801", "QF", "SDQ", "MIA", "2026-08-20 10:00", "2026-08-20 13:00", null),
            new FilaVueloImportacion(3, "QF802", "QF", "SDQ", "MIA", "2026-08-20 10:05", "2026-08-20 13:05", null)
        };

        // La segunda fila choca al guardar (p. ej. solape de puerta contra la ya importada).
        var vuelos = new FakeVuelos(
            validar: _ => [],
            registrar: cmd => cmd.Numero == "QF802"
                ? throw new InvalidOperationException("La puerta B5 ya está ocupada por el vuelo QF801.")
                : null);

        var service = new VueloImportacionService(new FakeLector(), new FakeCatalogo(), vuelos);

        var resultado = await service.ImportarFilasAsync(filas);

        Assert.Equal(2, resultado.Total);
        Assert.Equal(1, resultado.Importadas);
        Assert.True(resultado.Filas.Single(f => f.Fila.Linea == 2).Valido);
        var omitida = resultado.Filas.Single(f => f.Fila.Linea == 3);
        Assert.False(omitida.Valido);
        Assert.Contains(omitida.Errores, e => e.Contains("ya está ocupada"));
    }

    // ---------- Dobles de prueba ----------

    private sealed class FakeLector(params FilaVueloImportacion[] filas) : ILectorVuelosImportados
    {
        public IReadOnlyList<FilaVueloImportacion> Leer(Stream contenido, string nombreArchivo) => filas;
    }

    private sealed class FakeCatalogo : ICatalogoConsulta
    {
        public Task<Guid?> ObtenerAerolineaIdPorCodigoAsync(string codigo) =>
            Task.FromResult<Guid?>(codigo == "QF" ? Qf : null);
        public Task<Guid?> ObtenerAeropuertoIdPorCodigoAsync(string codigo) =>
            Task.FromResult<Guid?>(codigo switch { "SDQ" => Sdq, "MIA" => Mia, _ => null });
        public Task<PuertaResumen?> ObtenerPuertaPorCodigoAsync(string codigo) =>
            Task.FromResult<PuertaResumen?>(null);

        // No usados por la importación.
        public Task<bool> ExisteAerolineaAsync(Guid aerolineaId) => throw new NotSupportedException();
        public Task<bool> ExisteAeropuertoAsync(Guid aeropuertoId) => throw new NotSupportedException();
        public Task<Guid?> ObtenerAeropuertoBaseIdAsync() => throw new NotSupportedException();
        public Task<bool> ExistePuertaAsync(Guid puertaId) => throw new NotSupportedException();
        public Task<PuertaResumen?> ObtenerPuertaAsync(Guid puertaId) => throw new NotSupportedException();
    }

    private sealed class FakeVuelos(
        Func<RegistrarVueloCommand, IReadOnlyList<string>> validar,
        Func<RegistrarVueloCommand, object?>? registrar = null) : IVueloService
    {
        public Task<IReadOnlyList<string>> ValidarRegistroAsync(RegistrarVueloCommand command) =>
            Task.FromResult(validar(command));

        public Task<VueloDto> RegistrarAsync(RegistrarVueloCommand command)
        {
            registrar?.Invoke(command); // puede lanzar para simular un choque
            return Task.FromResult<VueloDto>(null!);
        }

        // No usados por estas pruebas.
        public Task<IReadOnlyList<VueloDto>> ObtenerTodosAsync() => throw new NotSupportedException();
        public Task<ResultadoPaginado<VueloDto>> ObtenerPaginadoAsync(int pagina, int tamano) => throw new NotSupportedException();
        public Task<IReadOnlyList<VueloDto>> ConsultarAsync(ConsultarVuelosQuery filtro) => throw new NotSupportedException();
        public Task<VueloDto?> ObtenerPorIdAsync(Guid id) => throw new NotSupportedException();
        public Task<VueloDto> ActualizarDatosAsync(Guid vueloId, ActualizarDatosVueloCommand command) => throw new NotSupportedException();
        public Task<VueloDto> CambiarEstadoAsync(Guid vueloId, ActualizarEstadoVueloCommand command) => throw new NotSupportedException();
        public Task<VueloDto> RegistrarCambioOperativoAsync(Guid vueloId, RegistrarCambioOperativoCommand command) => throw new NotSupportedException();
    }
}
