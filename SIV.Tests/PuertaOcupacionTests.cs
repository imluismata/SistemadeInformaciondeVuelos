using SIV.Modules.Vuelos.Application;
using SIV.Modules.Vuelos.Domain;
using SIV.Shared.Contracts;
using SIV.Shared.DTOs;
using SIV.Shared.Events;
using Xunit;

namespace SIV.Tests;

/// <summary>
/// Pruebas de la regla operativa "una puerta física atiende un vuelo a la vez":
/// se rechaza asignar una puerta ya ocupada por otro vuelo en una ventana solapada,
/// pero se permite reutilizarla en horarios que no se pisan. La rampa abierta (capacidad
/// múltiple) no se valida, y una puerta solo aplica a vuelos que tocan la base (SDQ).
/// Se ejercita a través de <see cref="VueloService"/> con el dominio real y dobles para
/// el resto; no hacen falta base de datos ni API.
/// </summary>
public class PuertaOcupacionTests
{
    private static readonly Guid BaseId = Guid.NewGuid();   // AILA / SDQ
    private static readonly Guid OtroAeropuerto = Guid.NewGuid();
    private static readonly Guid OtroAeropuerto2 = Guid.NewGuid();
    private static readonly Guid PuertaId = Guid.NewGuid();
    private static readonly Guid Aerolinea = Guid.NewGuid();

    [Fact]
    public async Task Registrar_PuertaOcupadaEnVentanaSolapada_Rechaza()
    {
        // Un vuelo ya sale de la puerta a las 10:00 (ocupa 09:30–10:00).
        var existente = Salida("EX100", new DateTime(2026, 8, 15, 10, 0, 0));
        var service = CrearServicio(EsRampa: false, existentes: existente);

        // Otro vuelo quiere salir de la MISMA puerta a las 10:10 (ocupa 09:40–10:10): solapan.
        var comando = SalidaCmd("NEW1", new DateTime(2026, 8, 15, 10, 10, 0));

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegistrarAsync(comando));
        Assert.Contains("ya está ocupada", ex.Message);
    }

    [Fact]
    public async Task Registrar_MismaPuertaEnHorariosSeparados_Permite()
    {
        // Un vuelo LLEGA a la puerta a las 16:00 (ocupa 16:00–16:30).
        var llegada = Llegada("EX200", new DateTime(2026, 8, 15, 16, 0, 0));
        var service = CrearServicio(EsRampa: false, existentes: llegada);

        // Otro vuelo SALE de la misma puerta a las 17:30 (ocupa 17:00–17:30): no se pisan.
        var comando = SalidaCmd("NEW2", new DateTime(2026, 8, 15, 17, 30, 0));

        var dto = await service.RegistrarAsync(comando); // no debe lanzar
        Assert.Equal("NEW2", dto.Numero);
    }

    [Fact]
    public async Task Registrar_RampaAbierta_NoValidaSolape()
    {
        // Aunque otro vuelo ocupe la rampa exactamente a la misma hora, se permite:
        // la rampa es posición de capacidad múltiple (válvula de escape para emergencias).
        var existente = Salida("EX300", new DateTime(2026, 8, 15, 12, 0, 0));
        var service = CrearServicio(EsRampa: true, existentes: existente);

        var comando = SalidaCmd("NEW3", new DateTime(2026, 8, 15, 12, 0, 0));

        var dto = await service.RegistrarAsync(comando); // no debe lanzar
        Assert.Equal("NEW3", dto.Numero);
    }

    [Fact]
    public async Task Registrar_VueloQueNoTocaLaBase_ConPuerta_Rechaza()
    {
        var service = CrearServicio(EsRampa: false);

        // Vuelo entre dos aeropuertos que no son la base, pero con puerta de la base asignada.
        var comando = new RegistrarVueloCommand(
            "NEW4", Aerolinea, OtroAeropuerto, OtroAeropuerto2,
            new DateTime(2026, 8, 15, 9, 0, 0), new DateTime(2026, 8, 15, 12, 0, 0), PuertaId);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => service.RegistrarAsync(comando));
        Assert.Contains("salen de o llegan a la base", ex.Message);
    }

    // ---------- Fábricas de vuelos/comandos ----------

    // Salida desde la base: ocupa la puerta [salida − 30 min, salida].
    private static RegistrarVueloCommand SalidaCmd(string numero, DateTime salida) =>
        new(numero, Aerolinea, BaseId, OtroAeropuerto, salida, salida.AddHours(2), PuertaId);

    private static Vuelo Salida(string numero, DateTime salida) =>
        new VueloDomainService().Registrar(numero, Aerolinea, BaseId, OtroAeropuerto,
            salida, salida.AddHours(2), PuertaId, "B5 · Terminal B");

    // Llegada a la base: ocupa la puerta [llegada, llegada + 30 min].
    private static Vuelo Llegada(string numero, DateTime llegada) =>
        new VueloDomainService().Registrar(numero, Aerolinea, OtroAeropuerto, BaseId,
            llegada.AddHours(-2), llegada, PuertaId, "B5 · Terminal B");

    private static VueloService CrearServicio(bool EsRampa, params Vuelo[] existentes) =>
        new(
            new FakeVueloRepositorio(existentes),
            new VueloDomainService(),
            new FakeAuditoria(),
            new FakePublicador(),
            new FakeUnitOfWork(),
            new FakeCatalogo(EsRampa));

    // ---------- Dobles de prueba ----------

    private sealed class FakeCatalogo(bool esRampa) : ICatalogoConsulta
    {
        public Task<bool> ExisteAerolineaAsync(Guid aerolineaId) => Task.FromResult(true);
        public Task<bool> ExisteAeropuertoAsync(Guid aeropuertoId) => Task.FromResult(true);
        public Task<bool> ExistePuertaAsync(Guid puertaId) => Task.FromResult(true);
        public Task<Guid?> ObtenerAeropuertoBaseIdAsync() => Task.FromResult<Guid?>(BaseId);
        public Task<PuertaResumen?> ObtenerPuertaAsync(Guid puertaId) =>
            Task.FromResult<PuertaResumen?>(new PuertaResumen(
                puertaId, "B5", esRampa ? null : "Terminal B", esRampa));

        // No usados por estas pruebas (resolución por código para importación).
        public Task<Guid?> ObtenerAerolineaIdPorCodigoAsync(string codigo) => throw new NotSupportedException();
        public Task<Guid?> ObtenerAeropuertoIdPorCodigoAsync(string codigo) => throw new NotSupportedException();
        public Task<PuertaResumen?> ObtenerPuertaPorCodigoAsync(string codigo) => throw new NotSupportedException();
    }

    private sealed class FakeVueloRepositorio(Vuelo[] enLaPuerta) : IVueloRepository
    {
        public Task<IReadOnlyList<Vuelo>> ObtenerActivosPorPuertaAsync(Guid puertaId, Guid? excluirVueloId) =>
            Task.FromResult<IReadOnlyList<Vuelo>>(enLaPuerta);

        public Task<bool> ExisteNumeroParaAerolineaYFechaAsync(string numero, Guid aerolineaId, DateTime fecha) =>
            Task.FromResult(false);

        public Task GuardarAsync(Vuelo vuelo) => Task.CompletedTask;

        // No usados por estas pruebas.
        public Task<IReadOnlyList<Vuelo>> ObtenerTodosAsync() => throw new NotSupportedException();
        public Task<(IReadOnlyList<Vuelo> Items, int Total)> ObtenerPaginadoAsync(int pagina, int tamano) => throw new NotSupportedException();
        public Task<IReadOnlyList<Vuelo>> ConsultarAsync(ConsultarVuelosQuery filtro) => throw new NotSupportedException();
        public Task<Vuelo?> ObtenerPorIdAsync(Guid id) => throw new NotSupportedException();
        public Task<bool> ExistenVuelosActivosPorAerolineaAsync(Guid aerolineaId) => throw new NotSupportedException();
        public Task<bool> ExistenVuelosActivosPorAeropuertoAsync(Guid aeropuertoId) => throw new NotSupportedException();
    }

    private sealed class FakeAuditoria : IAuditoriaService
    {
        public Task RegistrarAsync(string modulo, string accion, string resultado, string? detalle = null) => Task.CompletedTask;
        public Task<IReadOnlyList<AuditoriaDto>> ConsultarAsync(string? modulo, string? accion, DateTime? desde, DateTime? hasta) => throw new NotSupportedException();
    }

    private sealed class FakePublicador : IPublicadorEventos
    {
        public Task PublicarAsync(IVueloCambiadoEvento evento) => Task.CompletedTask;
    }

    // Ejecuta la operación sin transacción real: basta para probar la lógica.
    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion) => operacion();
        public Task EjecutarEnTransaccionAsync(Func<Task> operacion) => operacion();
    }
}
