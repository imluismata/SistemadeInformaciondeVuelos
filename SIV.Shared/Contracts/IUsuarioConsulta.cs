namespace SIV.Shared.Contracts;

/// <summary>
/// Contrato de consulta de usuarios para otros módulos. Permite obtener los datos
/// de contacto (nombre y correo) a partir de un conjunto de ids, sin exponer la
/// entidad interna Usuario. Cumple DA-02: la comunicación entre módulos ocurre a
/// través de contratos definidos en SIV.Shared.
/// </summary>
public interface IUsuarioConsulta
{
    Task<IReadOnlyList<UsuarioContacto>> ObtenerContactosAsync(IReadOnlyCollection<Guid> ids);
}

public sealed record UsuarioContacto(Guid Id, string Nombre, string Email);
