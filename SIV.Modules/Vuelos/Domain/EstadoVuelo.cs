namespace SIV.Modules.Vuelos.Domain;


// Un enum es el tipo correcto aquí porque los estados son un conjunto FINITO y CERRADO
// definido por el dominio (RN-EO-02 del SRS). No necesita ser una clase ni vivir en
// una tabla de base de datos separada: nunca va a cambiar dinámicamente en tiempo de
// ejecución, solo si el negocio decide agregar un estado nuevo (y eso requeriría
// recompilar el código de todas formas).
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
