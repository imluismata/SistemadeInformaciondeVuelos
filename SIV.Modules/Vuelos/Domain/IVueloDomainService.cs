using SIV.Shared.Enums;

namespace SIV.Modules.Vuelos.Domain;

internal interface IVueloDomainService
{
    Vuelo Registrar(
        string numero,
        Guid aerolineaId,
        Guid aeropuertoOrigenId,
        Guid aeropuertoDestinoId,
        DateTime horarioSalida,
        DateTime horarioLlegada,
        Guid? puertaId = null,
        string? puertaDescripcion = null);

    void CambiarEstado(Vuelo vuelo, EstadoVuelo nuevoEstado);

    void RegistrarRetraso(Vuelo vuelo, TimeSpan retraso, string motivo);

    void RegistrarAdelanto(Vuelo vuelo, TimeSpan adelanto, string motivo);

    // La existencia de la puerta la valida VueloService (vía ICatalogoConsulta) y le
    // pasa la descripción ya resuelta; el dominio solo aplica el cambio.
    void RegistrarCambioDePuerta(Vuelo vuelo, Guid? nuevaPuertaId, string? nuevaPuertaDescripcion, string motivo);

    void Cancelar(Vuelo vuelo, string motivo);

    void ActualizarDatos(
        Vuelo vuelo,
        Guid aerolineaId,
        Guid aeropuertoOrigenId,
        Guid aeropuertoDestinoId,
        DateTime horarioSalida,
        DateTime horarioLlegada,
        Guid? puertaId,
        string? puertaDescripcion,
        string motivo);
}
