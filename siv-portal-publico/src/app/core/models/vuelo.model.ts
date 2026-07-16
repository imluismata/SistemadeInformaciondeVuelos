export interface VueloPublico {
  id: string;
  aerolinea: string;
  origen: string;
  destino: string;
  horarioSalida: string;
  horarioLlegada: string;
  puerta: string | null;
  estado: string;
}

export interface FiltroConsulta {
  origen?: string | null;
  destino?: string | null;
  fecha?: string | null;
}
