namespace SIV.Modules.Seguimiento.Domain;

// representa cuando un usuario quiere recibir notificaciones de un vuelo
// use constructor privado y metodo Crear() para validar los datos antes de crear el objeto
public class Seguimiento
{
    public Guid Id { get; private set; }
    public Guid UsuarioId { get; private set; }
    public Guid VueloId { get; private set; }
    public EstadoSeguimiento Estado { get; private set; }
    public DateTime CreadoEn { get; private set; }
    public DateTime? CanceladoEn { get; private set; }

    private Seguimiento() { }

    public static Seguimiento Crear(Guid usuarioId, Guid vueloId)
    {
        if (usuarioId == Guid.Empty)
            throw new ArgumentException("El UsuarioId es obligatorio.");

        if (vueloId == Guid.Empty)
            throw new ArgumentException("El VueloId es obligatorio.");

        return new Seguimiento
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuarioId,
            VueloId = vueloId,
            Estado = EstadoSeguimiento.Activo,
            CreadoEn = DateTime.UtcNow,
            CanceladoEn = null
        };
    }

    // al cancelar guardamos la fecha para saber cuando ocurrio
    public void Cancelar()
    {
        if (Estado == EstadoSeguimiento.Cancelado)
            throw new InvalidOperationException("Este seguimiento ya fue cancelado.");

        Estado = EstadoSeguimiento.Cancelado;
        CanceladoEn = DateTime.UtcNow;
    }
}
