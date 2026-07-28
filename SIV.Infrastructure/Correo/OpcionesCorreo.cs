namespace SIV.Infrastructure.Correo;

/// <summary>
/// Configuración del envío de correos, leída de la sección "Correo" de appsettings.
/// </summary>
public sealed class OpcionesCorreo
{
    public const string Seccion = "Correo";

    public bool Habilitado { get; set; }
    public string Host { get; set; } = "smtp.gmail.com";
    public int Puerto { get; set; } = 587;
    public string Usuario { get; set; } = string.Empty;
    public string Clave { get; set; } = string.Empty;
    public string RemitenteNombre { get; set; } = "Quisqueya Flight Hub";
}
