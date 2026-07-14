export interface Seguimiento {
  id: string;
  usuarioId: string;
  vueloId: string;
  estado: string;
  creadoEn: string;
}

export interface RegistrarSeguimientoRequest {
  usuarioId: string;
  vueloId: string;
}

export interface CancelarSeguimientoRequest {
  usuarioId: string;
  vueloId: string;
}
