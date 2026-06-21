namespace SIV.Modules.Vuelos.Domain;

// ── ABSTRACCIÓN + HERENCIA (pilares de POO) ─────────────────────────────────
// Esta clase es "abstract": eso significa que NUNCA puedes hacer
// "new CambioOperativo(...)" directamente. Solo existe para que Retraso,
// Adelanto, CambioDePuerta y Cancelacion hereden de ella. ¿Por qué hacerla
// abstracta en vez de una clase normal? Porque conceptualmente, "un cambio
// operativo" sin más detalle no significa nada concreto en el dominio del
// SIV — siempre es UN TIPO específico de cambio. La clase abstracta captura
// lo que las 4 tienen en común (causa, fecha, valores) sin permitir que
// alguien cree una instancia "genérica" sin sentido.
public abstract class CambioOperativo
{
    // Estas propiedades son compartidas por las 4 subclases. Es código que NO
    // se repite en cada una de ellas gracias a la herencia — RN-CO-03 exige
    // que todo cambio tenga tipo, causa, valor anterior, valor nuevo y fecha,
    // y al ponerlo aquí una sola vez, las 4 subclases lo heredan gratis.
    public Guid Id { get; protected set; }
    public Guid VueloId { get; protected set; }
    public string Causa { get; protected set; } = string.Empty;
    public string? ValorAnterior { get; protected set; }
    public string? ValorNuevo { get; protected set; }
    public DateTime Registrado { get; protected set; }

    // "protected set" (no "private set" como en Vuelo.cs) porque aquí SÍ
    // queremos que las clases hijas (Retraso, Cancelacion, etc.) puedan asignar
    // estos valores desde sus propios constructores. Un código externo al
    // módulo sigue sin poder tocarlos, pero las subclases sí pueden.

    protected CambioOperativo(Guid vueloId, string causa)
    {
        if (string.IsNullOrWhiteSpace(causa))
            // RN-CO-02: todo cambio operativo debe tener una causa identificable.
            throw new ArgumentException("Todo cambio operativo debe tener una causa.", nameof(causa));

        Id = Guid.NewGuid();
        VueloId = vueloId;
        Causa = causa;
        Registrado = DateTime.UtcNow;
    }

    // Útil para que el repositorio sepa qué string guardar en la columna
    // "TipoCambio" de la base de datos (recuerda el ERD: usamos Table-Per-
    // Hierarchy, una sola tabla con esta columna discriminadora).
    public abstract string TipoCambio { get; }

    // ── EL MÉTODO CLAVE DEL POLIMORFISMO ─────────────────────────────────────
    // "abstract" significa que esta clase NO da una implementación: OBLIGA a
    // cada subclase a escribir su propia versión. Cuando Vuelo.cs llama a
    // "cambio.AplicarSobre(this)", C# decide en TIEMPO DE EJECUCIÓN cuál de
    // las 4 implementaciones ejecutar, según el tipo real del objeto. Eso es
    // el polimorfismo funcionando: el mismo método, 4 comportamientos.
    public abstract void AplicarSobre(Vuelo vuelo);
}
