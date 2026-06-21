namespace SIV.Modules.Notificaciones.Domain;

public class Notificacion
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid VueloId { get; private set; }
    public string Mensaje { get; private set; } = string.Empty;
    public string Estado { get; private set; } = "NoLeida";
    public DateTime GeneradaEn { get; private set; }
    public DateTime? LeidaEn { get; private set; }

    private Notificacion() { }

    public static Notificacion Crear(Guid usuarioId, Guid vueloId, string mensaje)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El usuarioId no puede estar vacío.");
        if (vueloId == Guid.Empty)
            throw new ArgumentException("El vueloId no puede estar vacío.");
        if (string.IsNullOrWhiteSpace(mensaje))
            throw new ArgumentException("El mensaje no puede estar vacío.");

        return new Notificacion
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            VueloId = vueloId,
            Mensaje = mensaje,
            Estado = "NoLeida",
            GeneradaEn = DateTime.UtcNow
        };
    }

    public void MarcarComoLeida()
    {
        if (Estado == "Leida") return;
        Estado = "Leida";
        LeidaEn = DateTime.UtcNow;
    }
}