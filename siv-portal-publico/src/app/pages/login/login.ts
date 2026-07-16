import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { UsuariosService } from '../../core/services/usuarios';
import { AuthService } from '../../core/services/auth';

@Component({
  selector: 'app-login',
  imports: [FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.scss',
})
export class Login {
  private readonly usuariosService = inject(UsuariosService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  email = '';
  password = '';
  readonly error = signal<string | null>(null);
  readonly enviando = signal(false);

  iniciarSesion(): void {
    this.error.set(null);
    this.enviando.set(true);

    this.usuariosService.login({ email: this.email, password: this.password }).subscribe({
      next: (usuario) => {
        this.auth.iniciarSesion(usuario);
        this.enviando.set(false);
        this.router.navigateByUrl('/');
      },
      error: () => {
        this.error.set('Email o contraseña incorrectos.');
        this.enviando.set(false);
      },
    });
  }
}
