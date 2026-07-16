export interface Usuario {
  id: string;
  nombre: string;
  email: string;
  rol: string;
  creadoEn: string;
}

export interface RegistroUsuarioRequest {
  nombre: string;
  email: string;
  password: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}
