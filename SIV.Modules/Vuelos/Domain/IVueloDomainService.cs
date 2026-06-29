namespace SIV.Modules.Vuelos.Domain;

public interface IVueloDomainService
{
    Vuelo Registrar(
        string numero,
        Guid aerolineaId,
        Guid aeropuertoOrigenId,
        Guid aeropuertoDestinoId,
        DateTime horarioSalida,
        DateTime horarioLlegada,
        string? puerta = null);

    void CambiarEstado(Vuelo vuelo, EstadoVuelo nuevoEstado);

    void RegistrarRetraso(Vuelo vuelo, TimeSpan retraso, string motivo);

    void RegistrarAdelanto(Vuelo vuelo, TimeSpan adelanto, string motivo);

    void RegistrarCambioDePuerta(Vuelo vuelo, string nuevaPuerta, string motivo);

    void Cancelar(Vuelo vuelo, string motivo);

    void ActualizarDatos(
        Vuelo vuelo,
        Guid aerolineaId,
        Guid aeropuertoOrigenId,
        Guid aeropuertoDestinoId,
        DateTime horarioSalida,
        DateTime horarioLlegada,
        string? puerta,
        string motivo);
}
