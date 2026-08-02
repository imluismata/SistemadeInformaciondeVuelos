namespace SIV.Shared.Enums;

// Estados del ciclo de vida del vuelo. Vive en SIV.Shared porque cruza la frontera
// del módulo Vuelos hacia la API (parámetros y comandos), de modo que ese cruce
// ocurre a través de un contrato compartido y no exponiendo el dominio (DA-02).
public enum EstadoVuelo
{
    Programado,
    Retrasado,
    Embarcando,
    EnVuelo,
    Aterrizado,
    Completado,
    Cancelado
}
