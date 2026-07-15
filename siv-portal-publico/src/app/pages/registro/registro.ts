import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { UsuariosService } from '../../core/services/usuarios';

@Component({
  selector: 'app-registro',
  imports: [FormsModule, RouterLink],
  templateUrl: './registro.html',
  styleUrl: './registro.scss',
})
export class Registro {
  private readonly usuariosService = inject(UsuariosService);
  private readonly router = inject(Router);

  nombre = '';
  email = '';
  password = '';
  readonly error = signal<string | null>(null);
  readonly enviando = signal(false);

  registrar(): void {
    this.error.set(null);
    this.enviando.set(true);

    this.usuariosService
      .registrar({ nombre: this.nombre, email: this.email, password: this.password })
      .subscribe({
        next: () => {
          this.enviando.set(false);
          this.router.navigateByUrl('/login');
        },
        error: () => {
          this.error.set('No se pudo completar el registro. Verifica los datos.');
          this.enviando.set(false);
        },
      });
  }
}
