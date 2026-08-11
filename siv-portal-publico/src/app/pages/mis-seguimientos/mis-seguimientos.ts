import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth';
import { SeguimientoService } from '../../core/services/seguimiento';
import { Seguimiento } from '../../core/models/seguimiento.model';

/**
 * Solo los vuelos que sigue el usuario. Los avisos de cambios viven aparte, en
 * /notificaciones — antes se mostraban también aquí en un panel propio, pero
 * era la misma información dos veces con dos diseños distintos.
 */
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
    if (!usuario) { this.router.navigateByUrl('/login'); return; }

    this.seguimientoService.obtenerPorUsuario(usuario.id).subscribe({
      next: (s) => { this.seguimientos.set(s); this.cargando.set(false); },
      error: () => this.cargando.set(false),
    });
  }

  cancelar(seguimiento: Seguimiento): void {
    this.seguimientoService
      .cancelar({ usuarioId: seguimiento.usuarioId, vueloId: seguimiento.vueloId })
      .subscribe(() =>
        this.seguimientos.update((lista) => lista.filter((s) => s.id !== seguimiento.id))
      );
  }

  getBadge(estado: string): string {
    const map: Record<string, string> = {
      'Activo': 'on-time', 'Cancelado': 'cancelled',
    };
    return map[estado] ?? 'default';
  }
}
