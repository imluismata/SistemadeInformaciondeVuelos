namespace SIV.Shared.Contracts;

/// <summary>
/// Abstracción de unidad de trabajo. Permite a un módulo ejecutar una operación
/// que abarca varios guardados (entidad + auditoría + notificaciones) dentro de
/// una única transacción atómica, sin conocer detalles de Entity Framework.
///
/// Vive en SIV.Shared para que cualquier módulo dependa de esta abstracción y no
/// de SIV.Infrastructure (DA-02 / inversión de dependencias). La implementación
/// concreta, que envuelve el SivDbContext compartido, vive en SIV.Infrastructure.
///
/// Materializa la promesa de DA-04 / RNF-TRZ-04: cambio de estado, cambio operativo
/// y notificación ocurren en la misma transacción; si algo falla, se revierte todo.
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Ejecuta <paramref name="operacion"/> dentro de una transacción y devuelve su
    /// resultado. Confirma (commit) si termina sin excepción; revierte (rollback) si
    /// se lanza cualquier excepción, que luego se propaga al llamador.
    /// </summary>
    Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion);

    /// <summary>
    /// Variante para operaciones sin valor de retorno.
    /// </summary>
    Task EjecutarEnTransaccionAsync(Func<Task> operacion);
}
