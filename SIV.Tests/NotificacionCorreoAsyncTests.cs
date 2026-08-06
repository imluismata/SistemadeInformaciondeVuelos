using Microsoft.Extensions.Logging.Abstractions;
using SIV.Modules.Notificaciones.Application.Services;
using SIV.Shared.Contracts;
using SIV.Shared.Enums;
using SIV.Shared.Events;
using Xunit;

namespace SIV.Tests;

/// <summary>
/// Pruebas del canal de correo asíncrono (RNF-REN-03). Verifican que el manejador del
/// evento de vuelo YA NO envía por SMTP dentro del flujo, sino que <b>encola</b> un
/// correo por destinatario para que el worker en segundo plano lo envíe fuera de la
/// transacción. Se usan dobles de prueba; no hacen falta base de datos ni SMTP.
/// </summary>
public class NotificacionCorreoAsyncTests
{
    [Fact]
    public async Task ManejarAsync_ConSeguidores_EncolaUnCorreoPorContacto()
    {
        // Arrange: dos usuarios siguen el vuelo, con sus contactos resueltos.
        var vueloId = Guid.NewGuid();
        var ana = Guid.NewGuid();
        var beto = Guid.NewGuid();

        var seguimiento = new FakeSeguimientoConsulta(new[] { ana, beto });
        var usuarios = new FakeUsuarioConsulta(new[]
        {
            new UsuarioContacto(ana, "Ana", "ana@test.com"),
            new UsuarioContacto(beto, "Beto", "beto@test.com"),
        });
        var cola = new FakeCola();

        var manejador = new ManejadorVueloCambiadoCorreo(
            seguimiento, usuarios, cola, NullLogger<ManejadorVueloCambiadoCorreo>.Instance);

        // Act
        await manejador.ManejarAsync(FakeEvento(vueloId, "AA123"));

        // Assert: se encoló un correo por contacto, con sus datos, sin envío sincrónico.
        Assert.Equal(2, cola.Encolados.Count);
        Assert.Contains(cola.Encolados, t => t.Destino == "ana@test.com" && t.Nombre == "Ana" && t.NumeroVuelo == "AA123");
        Assert.Contains(cola.Encolados, t => t.Destino == "beto@test.com" && t.Nombre == "Beto");
    }

    [Fact]
    public async Task ManejarAsync_SinSeguidores_NoEncolaNada()
    {
        // Arrange: nadie sigue el vuelo.
        var seguimiento = new FakeSeguimientoConsulta(Array.Empty<Guid>());
        var usuarios = new FakeUsuarioConsulta(Array.Empty<UsuarioContacto>());
        var cola = new FakeCola();

        var manejador = new ManejadorVueloCambiadoCorreo(
            seguimiento, usuarios, cola, NullLogger<ManejadorVueloCambiadoCorreo>.Instance);

        // Act
        await manejador.ManejarAsync(FakeEvento(Guid.NewGuid(), "BB456"));

        // Assert
        Assert.Empty(cola.Encolados);
    }

    // ---------- Dobles de prueba ----------

    private static IVueloCambiadoEvento FakeEvento(Guid vueloId, string numero) =>
        new EventoDePrueba(vueloId, numero);

    private sealed record EventoDePrueba(Guid VueloId, string NumeroVuelo) : IVueloCambiadoEvento
    {
        public string EstadoAnterior => "Programado";
        public string EstadoNuevo => "Retrasado";
        public TipoCambio TipoCambio => TipoCambio.Retraso;
        public string Causa => "Prueba";
        public DateTime OcurridoEn => DateTime.UtcNow;
    }

    private sealed class FakeSeguimientoConsulta(IEnumerable<Guid> ids) : ISeguimientoConsulta
    {
        private readonly IEnumerable<Guid> _ids = ids;
        public Task<IEnumerable<Guid>> ObtenerUsuariosPorVueloAsync(Guid vueloId) => Task.FromResult(_ids);
        public Task<IReadOnlyList<SeguidoresPorVuelo>> ContarSeguidoresActivosAsync() =>
            throw new NotSupportedException();
    }

    private sealed class FakeUsuarioConsulta(IReadOnlyList<UsuarioContacto> contactos) : IUsuarioConsulta
    {
        private readonly IReadOnlyList<UsuarioContacto> _contactos = contactos;
        public Task<IReadOnlyList<UsuarioContacto>> ObtenerContactosAsync(IReadOnlyCollection<Guid> ids) =>
            Task.FromResult(_contactos);
    }

    private sealed class FakeCola : IColaCorreos
    {
        public List<TrabajoCorreo> Encolados { get; } = new();
        public void Encolar(TrabajoCorreo trabajo) => Encolados.Add(trabajo);
        public IAsyncEnumerable<TrabajoCorreo> LeerTodosAsync(CancellationToken cancelacion) =>
            throw new NotSupportedException();
    }
}
