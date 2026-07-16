import { Injectable, signal } from '@angular/core';
import { Usuario } from '../models/usuario.model';

const STORAGE_KEY = 'siv_usuario_actual';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly usuarioActual = signal<Usuario | null>(this.cargarDeStorage());

  iniciarSesion(usuario: Usuario): void {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(usuario));
    this.usuarioActual.set(usuario);
  }

  cerrarSesion(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.usuarioActual.set(null);
  }

  estaAutenticado(): boolean {
    return this.usuarioActual() !== null;
  }

  private cargarDeStorage(): Usuario | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    return raw ? (JSON.parse(raw) as Usuario) : null;
  }
}
