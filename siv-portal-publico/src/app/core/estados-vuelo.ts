/**
 * Cómo se muestra al pasajero el estado de un vuelo.
 *
 * Las pantallas de aeropuerto no muestran el estado interno del sistema
 * ("Programado"), sino el rótulo que el pasajero entiende ("A tiempo"), y en
 * dos idiomas porque el público es local y turista.
 *
 * Es presentación derivada: el sistema sigue guardando un único estado. No
 * existe un estado "EnTiempo" en el dominio porque sería un duplicado exacto
 * de "Programado" — un vuelo programado que no se ha retrasado ES un vuelo a
 * tiempo. Traducirlo aquí evita tocar la máquina de estados y la base de datos.
 */
export interface EtiquetaEstado {
  /** Rótulo en español (el principal). */
  es: string;
  /** Rótulo en inglés (secundario, como en las pantallas reales). */
  en: string;
  /** Clase del badge definida en styles.scss. */
  clase: string;
}

/** Orden en que se ofrecen los estados para filtrar (el del ciclo de vida). */
export const ESTADOS_VUELO = [
  'Programado',
  'Retrasado',
  'Embarcando',
  'EnVuelo',
  'Aterrizado',
  'Completado',
  'Cancelado',
] as const;

const ETIQUETAS: Record<string, EtiquetaEstado> = {
  Programado: { es: 'A tiempo',   en: 'On Time',   clase: 'on-time' },
  Retrasado:  { es: 'Retrasado',  en: 'Delayed',   clase: 'delayed' },
  Embarcando: { es: 'Embarcando', en: 'Boarding',  clase: 'boarding' },
  EnVuelo:    { es: 'En vuelo',   en: 'In Flight', clase: 'boarding' },
  Aterrizado: { es: 'Aterrizado', en: 'Landed',    clase: 'arrived' },
  Completado: { es: 'Completado', en: 'Completed', clase: 'departed' },
  Cancelado:  { es: 'Cancelado',  en: 'Cancelled', clase: 'cancelled' },
};

/**
 * Traduce un estado del dominio a su rótulo de pantalla. Si llegara un estado
 * desconocido (por ejemplo, uno nuevo en la API), se muestra tal cual en vez
 * de dejar el hueco vacío.
 */
export function etiquetaEstado(estado: string): EtiquetaEstado {
  return ETIQUETAS[estado] ?? { es: estado, en: '', clase: 'default' };
}
