/**
 * Datos geográficos de los aeropuertos, buscados por su código IATA.
 *
 * La API ya devuelve el código y el nombre de cada aeropuerto; lo que no tiene
 * —y no le corresponde tener— son las coordenadas ni el nombre corto de la
 * ciudad. Eso es información de referencia fija (Miami no se va a mover), así
 * que vive aquí: hace falta para pedir el clima del destino y para escribir
 * "Miami (MIA)" en vez del nombre completo del aeropuerto.
 *
 * Si llega un aeropuerto que no está en la tabla, no pasa nada: se muestra el
 * nombre que manda la API y se omite el clima. Nunca se inventa un dato.
 */
export interface AeropuertoReferencia {
  ciudad: string;
  latitud: number;
  longitud: number;
}

const AEROPUERTOS: Record<string, AeropuertoReferencia> = {
  SDQ: { ciudad: 'Santo Domingo',      latitud: 18.4297, longitud: -69.6689 },
  MIA: { ciudad: 'Miami',              latitud: 25.7959, longitud: -80.2870 },
  JFK: { ciudad: 'Nueva York',         latitud: 40.6413, longitud: -73.7781 },
  PTY: { ciudad: 'Ciudad de Panamá',   latitud: 9.0714,  longitud: -79.3835 },
  BOG: { ciudad: 'Bogotá',             latitud: 4.7016,  longitud: -74.1469 },
  MEX: { ciudad: 'Ciudad de México',   latitud: 19.4363, longitud: -99.0721 },
  GDL: { ciudad: 'Guadalajara',        latitud: 20.5218, longitud: -103.3111 },
  ATL: { ciudad: 'Atlanta',            latitud: 33.6407, longitud: -84.4277 },
};

/** Datos de referencia del aeropuerto, o null si no lo conocemos. */
export function buscarAeropuerto(codigo: string | null | undefined): AeropuertoReferencia | null {
  if (!codigo) return null;
  return AEROPUERTOS[codigo.toUpperCase()] ?? null;
}

/**
 * Cómo se nombra un aeropuerto en una pantalla: la ciudad si la conocemos, y si
 * no el nombre completo que manda la API.
 */
export function nombreCorto(codigo: string, nombreCompleto: string): string {
  return buscarAeropuerto(codigo)?.ciudad ?? nombreCompleto;
}
