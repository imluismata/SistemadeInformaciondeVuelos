import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
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
  private readonly route = inject(ActivatedRoute);

  email = '';
  password = '';
  readonly error = signal<string | null>(null);
  readonly enviando = signal(false);
  readonly verificado = signal(this.route.snapshot.queryParamMap.get('verificado') === '1');
  readonly restablecida = signal(this.route.snapshot.queryParamMap.get('restablecida') === '1');

  iniciarSesion(): void {
    this.error.set(null);
    this.enviando.set(true);

    this.usuariosService.login({ email: this.email, password: this.password }).subscribe({
      next: (usuario) => {
        this.auth.iniciarSesion(usuario);
        this.enviando.set(false);
        this.router.navigateByUrl('/');
      },
      error: (e) => {
        this.enviando.set(false);
        // 403 = credenciales correctas pero correo sin verificar.
        if (e?.status === 403) {
          this.router.navigate(['/verificar'], { queryParams: { email: this.email } });
          return;
        }
        this.error.set('Email o contraseña incorrectos.');
      },
    });
  }
}
