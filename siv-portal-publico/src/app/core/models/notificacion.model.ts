export interface Notificacion {
  id: string;
  vueloId: string;
  mensaje: string;
  estado: string;
  generadaEn: string;
  leidaEn: string | null;
}
