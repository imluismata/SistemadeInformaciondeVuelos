using SIV.Shared.Exceptions;

namespace SIV.Modules.Vuelos.Domain;

// Esta es la entidad más importante del sistema. Es un "Aggregate Root" en términos
// de Domain-Driven Design: significa que es la única puerta de entrada para modificar
// tanto al Vuelo como a su historial de estados y sus cambios operativos. Nadie debe
// poder modificar el historial de un vuelo sin pasar por esta clase.
public class Vuelo
{
    // ── ENCAPSULAMIENTO (pilar de POO) ──────────────────────────────────────
    // Los setters son privados ("private set"). Esto significa que CUALQUIER código
    // fuera de esta clase puede LEER estas propiedades, pero nadie puede escribirlas
    // directamente desde afuera (ej: vuelo.EstadoActual = EstadoVuelo.Cancelado; NO
    // compila). La única forma de cambiar el estado es llamando a los métodos
    // públicos que SÍ validan las reglas de negocio antes de cambiar el valor.
    // Esto es exactamente lo que dice RN-EO-03: "no se puede saltar estados sin
    // cumplir las condiciones del estado anterior". El encapsulamiento es lo que
    // hace que esa regla sea imposible de romper, no solo una buena intención.
    public Guid Id { get; private set; }
    public string Numero { get; private set; } = string.Empty;
    public Guid AerolineaId { get; private set; }
    public Guid AeropuertoOrigenId { get; private set; }
    public Guid AeropuertoDestinoId { get; private set; }
    public DateTime HorarioSalida { get; private set; }
    public DateTime HorarioLlegada { get; private set; }
    public string? Puerta { get; private set; }
    public EstadoVuelo EstadoActual { get; private set; }
    public DateTime CreadoEn { get; private set; }

    // Estas dos colecciones son privadas y de solo lectura hacia afuera (ver más abajo
    // las propiedades públicas ObtenerHistorialEstados/ObtenerCambiosOperativos).
    // Esto implementa RN-EO-07 y RN-CO-07: el historial es INMUTABLE. Si expusiéramos
    // estas listas como "public List<...>", cualquiera podría hacer
    // vuelo.HistorialEstados.Clear() y borrar la auditoría completa. Al exponerlas
    // como IReadOnlyList, el código externo puede leerlas pero nunca modificarlas.
    private readonly List<HistorialEstado> _historialEstados = new();
    private readonly List<CambioOperativo> _cambiosOperativos = new();

    // ── ENCAPSULAMIENTO: constructor privado ────────────────────────────────
    // Nadie puede escribir "new Vuelo()" desde fuera de esta clase. ¿Por qué?
    // Porque un Vuelo recién creado con el constructor vacío de C# tendría todas
    // sus propiedades en valores por defecto (Guid.Empty, fechas en 0001-01-01),
    // lo cual viola RN-PV-02 (todo vuelo debe tener aerolínea, origen, destino y
    // horario). Forzamos a que la ÚNICA forma de crear un vuelo válido sea a
    // través del método estático Registrar(), que exige y valida esos datos.
    private Vuelo() { }

    // ── MÉTODO FACTORY (patrón de diseño, no es un pilar de POO en sí mismo,
    // pero se apoya en el encapsulamiento de arriba) ────────────────────────
    // Este método estático es la única puerta de entrada para crear un Vuelo.
    // Aquí viven las validaciones de RN-PV-02 y RN-PV-04 del SRS.
    public static Vuelo Registrar(
        string numero,
        Guid aerolineaId,
        Guid aeropuertoOrigenId,
        Guid aeropuertoDestinoId,
        DateTime horarioSalida,
        DateTime horarioLlegada)
    {
        // RN-PV-02: ningún dato obligatorio puede faltar.
        if (string.IsNullOrWhiteSpace(numero))
            throw new ArgumentException("El número de vuelo es obligatorio.", nameof(numero));

        // RN-PV-04: origen y destino no pueden ser el mismo aeropuerto.
        if (aeropuertoOrigenId == aeropuertoDestinoId)
            throw new ArgumentException("El aeropuerto de origen y destino no pueden ser el mismo.");

        if (horarioLlegada <= horarioSalida)
            throw new ArgumentException("El horario de llegada debe ser posterior al de salida.");

        var vuelo = new Vuelo
        {
            Id = Guid.NewGuid(),
            Numero = numero,
            AerolineaId = aerolineaId,
            AeropuertoOrigenId = aeropuertoOrigenId,
            AeropuertoDestinoId = aeropuertoDestinoId,
            HorarioSalida = horarioSalida,
            HorarioLlegada = horarioLlegada,
            CreadoEn = DateTime.UtcNow,
            // RF-EO-01: todo vuelo nuevo inicia en estado Programado automáticamente.
            EstadoActual = EstadoVuelo.Programado
        };

        // El primer registro de historial se crea aquí mismo, en el momento de
        // nacer el vuelo. Así el historial nunca empieza "vacío sin explicación".
        vuelo._historialEstados.Add(
            HistorialEstado.Registrar(vuelo.Id, EstadoVuelo.Programado, EstadoVuelo.Programado));

        return vuelo;
    }

    // ── MÁQUINA DE ESTADOS: el corazón de RN-EO-03, RN-EO-04, RN-EO-05 ──────
    // Este diccionario estático define, para cada estado, a qué estados SÍ puede
    // avanzar. Es la forma más simple y legible de implementar una máquina de
    // estados sin necesitar una librería externa. Vive como "static readonly"
    // porque esta regla es la misma para TODOS los vuelos del sistema, no cambia
    // de instancia a instancia.
    private static readonly Dictionary<EstadoVuelo, EstadoVuelo[]> TransicionesValidas = new()
    {
        [EstadoVuelo.Programado] = new[] { EstadoVuelo.Embarcando, EstadoVuelo.Retrasado, EstadoVuelo.Cancelado },
        [EstadoVuelo.Retrasado] = new[] { EstadoVuelo.Embarcando, EstadoVuelo.Cancelado },
        [EstadoVuelo.Embarcando] = new[] { EstadoVuelo.EnVuelo, EstadoVuelo.Cancelado },
        // RN-EO-05: no se puede cancelar un vuelo que ya está en vuelo o aterrizado.
        // Por eso EnVuelo solo puede avanzar a Aterrizado, nunca a Cancelado.
        [EstadoVuelo.EnVuelo] = new[] { EstadoVuelo.Aterrizado },
        [EstadoVuelo.Aterrizado] = new[] { EstadoVuelo.Completado },
        // RN-EO-04: Cancelado y Completado son estados TERMINALES.
        // Arrays vacíos = no hay transición válida hacia ningún lado.
        [EstadoVuelo.Completado] = Array.Empty<EstadoVuelo>(),
        [EstadoVuelo.Cancelado] = Array.Empty<EstadoVuelo>()
    };

    // Método público de comportamiento (no es un simple "setter"): el vuelo
    // SABE cómo avanzar su propio estado y SABE rechazar una transición inválida.
    // Esto es lo que distingue una entidad de dominio rica de un simple DTO.
    public void AvanzarEstado(EstadoVuelo nuevoEstado)
    {
        var estadoAnterior = EstadoActual;

        // RN-EO-03: valida contra el diccionario de transiciones válidas.
        if (!TransicionesValidas[estadoAnterior].Contains(nuevoEstado))
        {
            throw new TransicionInvalidaException(estadoAnterior.ToString(), nuevoEstado.ToString());
        }

        EstadoActual = nuevoEstado;

        // RN-EO-06: cada transición se registra en el historial automáticamente.
        // El operador nunca tiene que "acordarse" de registrar el cambio; el
        // propio objeto Vuelo lo garantiza siempre, sin excepción.
        _historialEstados.Add(HistorialEstado.Registrar(Id, estadoAnterior, nuevoEstado));
    }

    // ── POLIMORFISMO (pilar de POO) ──────────────────────────────────────────
    // Fíjate que este método recibe un "CambioOperativo" (el tipo abstracto/base),
    // no un "Retraso" o "CambioDePuerta" específico. El Vuelo no necesita saber
    // de qué subtipo concreto se trata: solo le pide al cambio que se aplique
    // sobre sí mismo llamando a cambio.AplicarSobre(this). Cada subclase
    // (Retraso, Cancelacion, CambioDePuerta, Adelanto) implementa ese método de
    // forma distinta. Este único método de Vuelo funciona para los 4 tipos de
    // cambio sin necesitar un "if (cambio is Retraso) ... else if (cambio is
    // Cancelacion) ..." — eso es exactamente lo que el polimorfismo evita.
    public void AplicarCambioOperativo(CambioOperativo cambio)
    {
        // RN-CO-04: no se permiten cambios operativos sobre un vuelo Cancelado
        // o Aterrizado.
        if (EstadoActual is EstadoVuelo.Cancelado or EstadoVuelo.Aterrizado or EstadoVuelo.Completado)
        {
            throw new InvalidOperationException(
                $"No se pueden registrar cambios operativos sobre un vuelo en estado {EstadoActual}.");
        }

        // Aquí ocurre el polimorfismo: el comportamiento exacto de "aplicar" depende
        // del tipo real del objeto en tiempo de ejecución, no del tipo declarado aquí.
        cambio.AplicarSobre(this);

        _cambiosOperativos.Add(cambio);
    }

    // Método interno usado solo por las clases de CambioOperativo (en el mismo
    // ensamblado) para modificar horario o puerta sin exponer setters públicos
    // a cualquier código externo al módulo. "internal" restringe el acceso a
    // SIV.Modules únicamente.
    internal void ActualizarHorarioSalida(DateTime nuevoHorario) => HorarioSalida = nuevoHorario;
    internal void ActualizarPuerta(string nuevaPuerta) => Puerta = nuevaPuerta;
    internal void MarcarComoCancelado() => AvanzarEstado(EstadoVuelo.Cancelado);

    // ── ENCAPSULAMIENTO: exposición controlada de las colecciones privadas ──
    // IReadOnlyList permite leer (recorrer con foreach, usar Count, indexar) pero
    // no permite Add/Remove/Clear. Así protegemos la inmutabilidad sin tener que
    // copiar la lista completa cada vez que alguien la consulta.
    public IReadOnlyList<HistorialEstado> ObtenerHistorialEstados() => _historialEstados;
    public IReadOnlyList<CambioOperativo> ObtenerCambiosOperativos() => _cambiosOperativos;
}
