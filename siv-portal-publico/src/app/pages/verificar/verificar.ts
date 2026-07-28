import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { UsuariosService } from '../../core/services/usuarios';

@Component({
  selector: 'app-verificar',
  imports: [FormsModule, RouterLink],
  templateUrl: './verificar.html',
  styleUrl: './verificar.scss',
})
export class Verificar implements OnInit {
  private readonly usuariosService = inject(UsuariosService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  email = '';
  codigo = '';
  readonly error = signal<string | null>(null);
  readonly aviso = signal<string | null>(null);
  readonly enviando = signal(false);

  ngOnInit(): void {
    this.email = this.route.snapshot.queryParamMap.get('email') ?? '';
  }

  verificar(): void {
    this.error.set(null);
    this.aviso.set(null);
    this.enviando.set(true);

    this.usuariosService.verificarCodigo(this.email, this.codigo.trim()).subscribe({
      next: () => {
        this.enviando.set(false);
        this.router.navigate(['/login'], { queryParams: { verificado: '1' } });
      },
      error: (e) => {
        this.enviando.set(false);
        this.error.set(e?.error?.error ?? 'No se pudo verificar el código.');
      },
    });
  }

  reenviar(): void {
    this.error.set(null);
    this.aviso.set(null);
    this.usuariosService.reenviarCodigo(this.email).subscribe({
      next: () => this.aviso.set('Te enviamos un nuevo código a tu correo.'),
      error: (e) => this.error.set(e?.error?.error ?? 'No se pudo reenviar el código.'),
    });
  }
}
