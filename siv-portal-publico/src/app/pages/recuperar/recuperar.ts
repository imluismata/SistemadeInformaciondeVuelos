import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { UsuariosService } from '../../core/services/usuarios';

@Component({
  selector: 'app-recuperar',
  imports: [FormsModule, RouterLink],
  templateUrl: './recuperar.html',
  styleUrl: './recuperar.scss',
})
export class Recuperar {
  private readonly usuariosService = inject(UsuariosService);
  private readonly router = inject(Router);

  // fase 1: pedir el código; fase 2: validar el código; fase 3: nueva contraseña
  readonly fase = signal<1 | 2 | 3>(1);

  email = '';
  codigo = '';
  nuevaPassword = '';

  readonly error = signal<string | null>(null);
  readonly enviando = signal(false);

  solicitar(): void {
    this.error.set(null);
    this.enviando.set(true);

    this.usuariosService.recuperarPassword(this.email).subscribe({
      next: () => {
        this.enviando.set(false);
        this.fase.set(2);
      },
      error: () => {
        this.enviando.set(false);
        this.error.set('No se pudo procesar la solicitud. Intenta de nuevo.');
      },
    });
  }

  validarCodigo(): void {
    this.error.set(null);
    this.enviando.set(true);

    this.usuariosService.validarCodigo(this.email, this.codigo.trim()).subscribe({
      next: () => {
        this.enviando.set(false);
        this.fase.set(3);
      },
      error: (e) => {
        this.enviando.set(false);
        this.error.set(e?.error?.error ?? 'El código no es válido.');
      },
    });
  }

  restablecer(): void {
    this.error.set(null);
    this.enviando.set(true);

    this.usuariosService
      .restablecerPassword(this.email, this.codigo.trim(), this.nuevaPassword)
      .subscribe({
        next: () => {
          this.enviando.set(false);
          this.router.navigate(['/login'], { queryParams: { restablecida: '1' } });
        },
        error: (e) => {
          this.enviando.set(false);
          this.error.set(e?.error?.error ?? 'No se pudo restablecer la contraseña.');
        },
      });
  }
}
