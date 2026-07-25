namespace SIV.Modules.Usuarios.Application.Interfaces;

/// <summary>
/// Abstracción del envío de correos. La implementación concreta (SMTP) vive en
/// la capa de infraestructura; el módulo de Usuarios solo depende de este contrato.
/// </summary>
public interface IServicioCorreo
{
    Task EnviarCodigoVerificacionAsync(string destino, string nombre, string codigo);
    Task EnviarCodigoRecuperacionAsync(string destino, string nombre, string codigo);
}
