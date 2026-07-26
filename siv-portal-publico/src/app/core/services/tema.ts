import { Injectable, signal } from '@angular/core';

type Tema = 'claro' | 'oscuro';
const STORAGE_KEY = 'siv_tema';

/**
 * Controla el tema claro/oscuro del portal. Guarda la preferencia en
 * localStorage y aplica el atributo data-theme al <html>, que las variables
 * CSS usan para cambiar los colores.
 */
@Injectable({ providedIn: 'root' })
export class TemaService {
  readonly tema = signal<Tema>(this.temaInicial());

  constructor() {
    this.aplicar(this.tema());
  }

  alternar(): void {
    const nuevo: Tema = this.tema() === 'oscuro' ? 'claro' : 'oscuro';
    this.tema.set(nuevo);
    localStorage.setItem(STORAGE_KEY, nuevo);
    this.aplicar(nuevo);
  }

  private aplicar(tema: Tema): void {
    document.documentElement.setAttribute('data-theme', tema === 'oscuro' ? 'dark' : 'light');
  }

  private temaInicial(): Tema {
    const guardado = localStorage.getItem(STORAGE_KEY);
    if (guardado === 'claro' || guardado === 'oscuro') return guardado;
    // Si no hay preferencia guardada, respeta la del sistema.
    return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'oscuro' : 'claro';
  }
}
