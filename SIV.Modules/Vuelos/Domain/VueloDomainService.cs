using SIV.Shared.Exceptions;

namespace SIV.Modules.Vuelos.Domain;

public sealed class VueloDomainService : IVueloDomainService
{
    private static readonly IReadOnlyDictionary<EstadoVuelo, EstadoVuelo[]> TransicionesValidas =
        new Dictionary<EstadoVuelo, EstadoVuelo[]>
        {
            [EstadoVuelo.Programado]  = [EstadoVuelo.Retrasado, EstadoVuelo.Embarcando, EstadoVuelo.Cancelado],
            [EstadoVuelo.Retrasado]   = [EstadoVuelo.Programado, EstadoVuelo.Embarcando, EstadoVuelo.Cancelado],
            [EstadoVuelo.Embarcando]  = [EstadoVuelo.EnVuelo, EstadoVuelo.Cancelado],
            [EstadoVuelo.EnVuelo]     = [EstadoVuelo.Aterrizado],
            [EstadoVuelo.Aterrizado]  = [EstadoVuelo.Completado],
            [EstadoVuelo.Completado]  = Array.Empty<EstadoVuelo>(),
            [EstadoVuelo.Cancelado]   = Array.Empty<EstadoVuelo>()
        };

    public Vuelo Registrar(
        string numero,
        Guid aerolineaId,
        Guid aeropuertoOrigenId,
        Guid aeropuertoDestinoId,
        DateTime horarioSalida,
        DateTime horarioLlegada,
        string? puerta = null)
    {
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("El número de vuelo es obligatorio.", nameof(numero));

        if (aerolineaId == Guid.Empty)
            throw new ArgumentException("La aerolínea es obligatoria.", nameof(aerolineaId));

        if (aeropuertoOrigenId == Guid.Empty || aeropuertoDestinoId == Guid.Empty)
            throw new ArgumentException("Los aeropuertos de origen y destino son obligatorios.");

        if (aeropuertoOrigenId == aeropuertoDestinoId)
            throw new ArgumentException("El aeropuerto de origen y destino no pueden ser el mismo.");

        if (horarioLlegada <= horarioSalida)
            throw new ArgumentException("El horario de llegada debe ser posterior al de salida.");

        var vuelo = Vuelo.Crear();
        vuelo.Id = Guid.NewGuid();
        vuelo.Numero = numero.Trim();
        vuelo.AerolineaId = aerolineaId;
        vuelo.AeropuertoOrigenId = aeropuertoOrigenId;
        vuelo.AeropuertoDestinoId = aeropuertoDestinoId;
        vuelo.HorarioSalida = horarioSalida;
        vuelo.HorarioLlegada = horarioLlegada;
        vuelo.Puerta = string.IsNullOrWhiteSpace(puerta) ? null : puerta.Trim();
        vuelo.EstadoActual = EstadoVuelo.Programado;
        vuelo.CreadoEn = DateTime.UtcNow;

        vuelo.AgregarHistorial(new HistorialEstado(
            Guid.NewGuid(), vuelo.Id, EstadoVuelo.Programado, EstadoVuelo.Programado, vuelo.CreadoEn));

        return vuelo;
    }

    public void CambiarEstado(Vuelo vuelo, EstadoVuelo nuevoEstado)
    {
        if (vuelo.EstadoActual is EstadoVuelo.Completado or EstadoVuelo.Cancelado)
            throw new InvalidOperationException($"No se puede cambiar un vuelo en estado {vuelo.EstadoActual}.");

        if (!TransicionesValidas[vuelo.EstadoActual].Contains(nuevoEstado))
            throw new TransicionInvalidaException(vuelo.EstadoActual.ToString(), nuevoEstado.ToString());

        if (nuevoEstado == vuelo.EstadoActual)
            return;

        var estadoAnterior = vuelo.EstadoActual;
        vuelo.EstadoActual = nuevoEstado;
        vuelo.AgregarHistorial(new HistorialEstado(Guid.NewGuid(), vuelo.Id, estadoAnterior, nuevoEstado, DateTime.UtcNow));
    }

    public void RegistrarRetraso(Vuelo vuelo, TimeSpan retraso, string motivo)
    {
        ValidarMotivo(motivo);

        if (retraso <= TimeSpan.Zero)
            throw new ArgumentException("El retraso debe ser positivo.", nameof(retraso));

        AjustarTiempo(vuelo, TipoCambioOperativo.Retraso, retraso, motivo, moverHaciaAdelante: true);
    }

    public void RegistrarAdelanto(Vuelo vuelo, TimeSpan adelanto, string motivo)
    {
        ValidarMotivo(motivo);

        if (adelanto <= TimeSpan.Zero)
            throw new ArgumentException("El adelanto debe ser positivo.", nameof(adelanto));

        AjustarTiempo(vuelo, TipoCambioOperativo.Adelanto, adelanto, motivo, moverHaciaAdelante: false);
    }

    public void RegistrarCambioDePuerta(Vuelo vuelo, string nuevaPuerta, string motivo)
    {
        ValidarMotivo(motivo);
        AsegurarEstadoOperativo(vuelo);

        if (string.IsNullOrWhiteSpace(nuevaPuerta))
            throw new ArgumentException("La nueva puerta es obligatoria.", nameof(nuevaPuerta));

        var anterior = vuelo.Puerta ?? "(sin asignar)";
        vuelo.Puerta = nuevaPuerta.Trim();

        vuelo.AgregarCambioOperativo(CrearCambio(vuelo.Id, TipoCambioOperativo.CambioDePuerta, motivo, anterior, vuelo.Puerta));
    }

    public void Cancelar(Vuelo vuelo, string motivo)
    {
        ValidarMotivo(motivo);
        AsegurarEstadoOperativo(vuelo);

        vuelo.AgregarCambioOperativo(CrearCambio(vuelo.Id, TipoCambioOperativo.Cancelacion, motivo,
            vuelo.EstadoActual.ToString(), EstadoVuelo.Cancelado.ToString()));

        CambiarEstado(vuelo, EstadoVuelo.Cancelado);
    }

    public void ActualizarDatos(
        Vuelo vuelo,
        Guid aerolineaId,
        Guid aeropuertoOrigenId,
        Guid aeropuertoDestinoId,
        DateTime horarioSalida,
        DateTime horarioLlegada,
        string? puerta,
        string motivo)
    {
        if (aerolineaId == Guid.Empty)
            throw new ArgumentException("La aerolínea es obligatoria.", nameof(aerolineaId));

        if (aeropuertoOrigenId == Guid.Empty || aeropuertoDestinoId == Guid.Empty)
            throw new ArgumentException("Los aeropuertos de origen y destino son obligatorios.");

        if (aeropuertoOrigenId == aeropuertoDestinoId)
            throw new ArgumentException("El aeropuerto de origen y destino no pueden ser el mismo.");

        if (horarioLlegada <= horarioSalida)
            throw new ArgumentException("El horario de llegada debe ser posterior al de salida.");

        ValidarMotivo(motivo);

        var valorAnterior = $"Aerolinea={vuelo.AerolineaId};Origen={vuelo.AeropuertoOrigenId};Destino={vuelo.AeropuertoDestinoId};Salida={vuelo.HorarioSalida:o};Llegada={vuelo.HorarioLlegada:o};Puerta={vuelo.Puerta ?? "(sin asignar)"}";

        vuelo.AerolineaId = aerolineaId;
        vuelo.AeropuertoOrigenId = aeropuertoOrigenId;
        vuelo.AeropuertoDestinoId = aeropuertoDestinoId;
        vuelo.HorarioSalida = horarioSalida;
        vuelo.HorarioLlegada = horarioLlegada;
        vuelo.Puerta = string.IsNullOrWhiteSpace(puerta) ? null : puerta.Trim();

        var valorNuevo = $"Aerolinea={vuelo.AerolineaId};Origen={vuelo.AeropuertoOrigenId};Destino={vuelo.AeropuertoDestinoId};Salida={vuelo.HorarioSalida:o};Llegada={vuelo.HorarioLlegada:o};Puerta={vuelo.Puerta ?? "(sin asignar)"}";

        vuelo.AgregarCambioOperativo(CrearCambio(vuelo.Id, TipoCambioOperativo.ActualizacionDatos, motivo, valorAnterior, valorNuevo));
    }

    private void AjustarTiempo(Vuelo vuelo, TipoCambioOperativo tipo, TimeSpan duracion, string motivo, bool moverHaciaAdelante)
    {
        AsegurarEstadoOperativo(vuelo);

        var anteriorSalida = vuelo.HorarioSalida;
        var anteriorLlegada = vuelo.HorarioLlegada;

        vuelo.HorarioSalida = moverHaciaAdelante ? vuelo.HorarioSalida.Add(duracion) : vuelo.HorarioSalida.Subtract(duracion);
        vuelo.HorarioLlegada = moverHaciaAdelante ? vuelo.HorarioLlegada.Add(duracion) : vuelo.HorarioLlegada.Subtract(duracion);

        if (vuelo.HorarioLlegada <= vuelo.HorarioSalida)
        {
            vuelo.HorarioSalida = anteriorSalida;
            vuelo.HorarioLlegada = anteriorLlegada;
            throw new InvalidOperationException("El ajuste de horarios dejaría el vuelo con tiempos inválidos.");
        }

        if (tipo == TipoCambioOperativo.Retraso && vuelo.EstadoActual == EstadoVuelo.Programado)
            CambiarEstado(vuelo, EstadoVuelo.Retrasado);

        vuelo.AgregarCambioOperativo(CrearCambio(vuelo.Id, tipo, motivo,
            $"Salida={anteriorSalida:o}; Llegada={anteriorLlegada:o}",
            $"Salida={vuelo.HorarioSalida:o}; Llegada={vuelo.HorarioLlegada:o}"));
    }

    private static void ValidarMotivo(string motivo)
    {
        if (string.IsNullOrWhiteSpace(motivo))
            throw new ArgumentException("El motivo es obligatorio.", nameof(motivo));
    }

    private static void AsegurarEstadoOperativo(Vuelo vuelo)
    {
        if (vuelo.EstadoActual is EstadoVuelo.Cancelado or EstadoVuelo.Completado or EstadoVuelo.Aterrizado)
            throw new InvalidOperationException($"No se pueden registrar cambios sobre un vuelo en estado {vuelo.EstadoActual}.");
    }

    private static CambioOperativo CrearCambio(Guid vueloId, TipoCambioOperativo tipo, string motivo, string? valorAnterior, string? valorNuevo)
        => new(Guid.NewGuid(), vueloId, tipo, motivo.Trim(), valorAnterior, valorNuevo, DateTime.UtcNow);
}
