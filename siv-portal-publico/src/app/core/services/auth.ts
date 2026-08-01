import { Injectable, signal } from '@angular/core';
import { Usuario } from '../models/usuario.model';

const USUARIO_KEY = 'siv_usuario_actual';
const TOKEN_KEY = 'siv_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly usuarioActual = signal<Usuario | null>(this.cargarUsuario());
  private token: string | null = localStorage.getItem(TOKEN_KEY);

  iniciarSesion(usuario: Usuario, token: string): void {
    localStorage.setItem(USUARIO_KEY, JSON.stringify(usuario));
    localStorage.setItem(TOKEN_KEY, token);
    this.token = token;
    this.usuarioActual.set(usuario);
  }

  cerrarSesion(): void {
    localStorage.removeItem(USUARIO_KEY);
    localStorage.removeItem(TOKEN_KEY);
    this.token = null;
    this.usuarioActual.set(null);
  }

  /** Token JWT del usuario autenticado, o null si no hay sesión. */
  obtenerToken(): string | null {
    return this.token;
  }

  estaAutenticado(): boolean {
    return this.usuarioActual() !== null;
  }

  private cargarUsuario(): Usuario | null {
    const raw = localStorage.getItem(USUARIO_KEY);
    return raw ? (JSON.parse(raw) as Usuario) : null;
  }
}
