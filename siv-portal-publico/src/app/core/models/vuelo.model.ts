export interface VueloPublico {
  id: string;
  numero: string;
  aerolinea: string;
  /** Código IATA de la aerolínea: AA, B6, DM... */
  aerolineaCodigo: string;
  origen: string;
  /** Código IATA del aeropuerto de origen: SDQ, MIA, JFK... */
  origenCodigo: string;
  destino: string;
  destinoCodigo: string;
  /** Hora a la que el vuelo sale ahora mismo (la estimada, si hubo retraso). */
  horarioSalida: string;
  horarioLlegada: string;
  puerta: string | null;
  estado: string;

  /**
   * Horas originalmente programadas, si el vuelo fue movido.
   *
   * Todavía no las envía la API: hoy el dominio guarda un solo par de horas y,
   * al retrasar un vuelo, las adelanta perdiendo de vista las originales. Las
   * pantallas ya saben pintarlas (la programada tachada y la nueva en rojo),
   * así que el día que la API las mande no habrá que tocar la vista.
   */
  horarioSalidaOriginal?: string | null;
  horarioLlegadaOriginal?: string | null;
}

export interface FiltroConsulta {
  origen?: string | null;
  destino?: string | null;
  fecha?: string | null;
}
