using Microsoft.EntityFrameworkCore;
using SIV.Shared.Contracts;

namespace SIV.Infrastructure;

/// <summary>
/// Implementación de <see cref="IUnitOfWork"/> sobre el SivDbContext compartido.
/// Abre una transacción de base de datos, ejecuta la operación y hace un único
/// commit al final; ante cualquier excepción hace rollback y re-lanza.
///
/// Como el DbContext es único y compartido por todos los módulos (DA-04), los
/// SaveChanges que ejecuten los distintos repositorios dentro de la operación
/// (vuelo, auditoría, notificaciones) se enlistan en esta misma transacción.
/// </summary>
internal sealed class UnitOfWork(SivDbContext db) : IUnitOfWork
{
    public async Task<T> EjecutarEnTransaccionAsync<T>(Func<Task<T>> operacion)
    {
        // Si ya hay una transacción activa (operación anidada), no abrimos otra:
        // reutilizamos la existente para que el commit lo controle el nivel externo.
        if (db.Database.CurrentTransaction is not null)
            return await operacion();

        await using var transaccion = await db.Database.BeginTransactionAsync();
        try
        {
            var resultado = await operacion();
            await transaccion.CommitAsync();
            return resultado;
        }
        catch
        {
            await transaccion.RollbackAsync();
            throw;
        }
    }

    public Task EjecutarEnTransaccionAsync(Func<Task> operacion)
        // Reutiliza la versión genérica devolviendo un valor ficticio para no duplicar lógica.
        => EjecutarEnTransaccionAsync(async () =>
        {
            await operacion();
            return true;
        });
}
