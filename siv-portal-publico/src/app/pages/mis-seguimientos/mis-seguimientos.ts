import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth';
import { SeguimientoService } from '../../core/services/seguimiento';
import { Seguimiento } from '../../core/models/seguimiento.model';

@Component({
  selector: 'app-mis-seguimientos',
  imports: [CommonModule, RouterLink],
  templateUrl: './mis-seguimientos.html',
  styleUrl: './mis-seguimientos.scss',
})
export class MisSeguimientos implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly seguimientoService = inject(SeguimientoService);
  private readonly router = inject(Router);

  readonly seguimientos = signal<Seguimiento[]>([]);
  readonly cargando = signal(true);

  ngOnInit(): void {
    const usuario = this.auth.usuarioActual();
    if (!usuario) {
      this.router.navigateByUrl('/login');
      return;
    }

    this.seguimientoService.obtenerPorUsuario(usuario.id).subscribe({
      next: (seguimientos) => {
        this.seguimientos.set(seguimientos);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false),
    });
  }

  cancelar(seguimiento: Seguimiento): void {
    this.seguimientoService
      .cancelar({ usuarioId: seguimiento.usuarioId, vueloId: seguimiento.vueloId })
      .subscribe(() => {
        this.seguimientos.update((lista) => lista.filter((s) => s.id !== seguimiento.id));
      });
  }
}
