import { Component, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { UsuariosService } from '../../core/services/usuarios';
import { mensajeDeError } from '../../core/errores';

/**
 * Mismo mínimo que exige la API en el request de registro. Aquí se repite a
 * propósito: la validación del cliente es comodidad (avisa al instante, sin ir
 * al servidor), la del servidor es la que manda. Si alguien salta el formulario,
 * la API sigue rechazando la contraseña corta.
 */
export const MINIMO_PASSWORD = 8;

@Component({
  selector: 'app-registro',
  imports: [FormsModule, RouterLink],
  templateUrl: './registro.html',
  styleUrl: './registro.scss',
})
export class Registro {
  private readonly usuariosService = inject(UsuariosService);
  private readonly router = inject(Router);

  readonly minimoPassword = MINIMO_PASSWORD;

  readonly nombre = signal('');
  readonly email = signal('');
  readonly password = signal('');

  readonly error = signal<string | null>(null);
  readonly enviando = signal(false);

  /** Cuántos caracteres le faltan a la contraseña; 0 cuando ya cumple. */
  readonly faltan = computed(() => Math.max(0, MINIMO_PASSWORD - this.password().length));

  /**
   * Mientras no esté completo, el botón queda deshabilitado: es mejor no dejar
   * intentarlo que dejar intentarlo y devolver un error.
   */
  readonly puedeEnviar = computed(() =>
    this.nombre().trim().length > 0 &&
    this.email().trim().length > 0 &&
    this.faltan() === 0
  );

  registrar(): void {
    if (!this.puedeEnviar() || this.enviando()) return;

    this.error.set(null);
    this.enviando.set(true);

    this.usuariosService
      .registrar({ nombre: this.nombre(), email: this.email(), password: this.password() })
      .subscribe({
        next: () => {
          this.enviando.set(false);
          this.router.navigate(['/verificar'], { queryParams: { email: this.email() } });
        },
        // Antes se mostraba siempre el mismo texto genérico y el usuario no sabía
        // qué corregir; ahora se enseña el motivo real que devuelve la API
        // (email ya registrado, formato inválido, demasiados intentos...).
        error: (e) => {
          this.error.set(mensajeDeError(e, 'No se pudo completar el registro. Verifica los datos.'));
          this.enviando.set(false);
        },
      });
  }
}
