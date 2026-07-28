import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth';
import { SeguimientoService } from '../../core/services/seguimiento';
import { NotificacionesService } from '../../core/services/notificaciones';
import { Seguimiento } from '../../core/models/seguimiento.model';
import { Notificacion } from '../../core/models/notificacion.model';

@Component({
  selector: 'app-mis-seguimientos',
  imports: [CommonModule, RouterLink],
  templateUrl: './mis-seguimientos.html',
  styleUrl: './mis-seguimientos.scss',
})
export class MisSeguimientos implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly seguimientoService = inject(SeguimientoService);
  private readonly notificacionesService = inject(NotificacionesService);
  private readonly router = inject(Router);

  readonly seguimientos = signal<Seguimiento[]>([]);
  readonly cargando = signal(true);

  readonly notificaciones = signal<Notificacion[]>([]);
  readonly noLeidas = computed(() =>
    this.notificaciones().filter((n) => n.leidaEn === null)
  );

  ngOnInit(): void {
    const usuario = this.auth.usuarioActual();
    if (!usuario) { this.router.navigateByUrl('/login'); return; }

    this.seguimientoService.obtenerPorUsuario(usuario.id).subscribe({
      next: (s) => { this.seguimientos.set(s); this.cargando.set(false); },
      error: () => this.cargando.set(false),
    });

    this.notificacionesService.obtenerPorUsuario(usuario.id).subscribe({
      next: (n) => this.notificaciones.set(
        n.sort((a, b) => +new Date(b.generadaEn) - +new Date(a.generadaEn))
      ),
      error: () => {},
    });
  }

  descartar(notificacion: Notificacion): void {
    this.notificacionesService.marcarComoLeida(notificacion.id).subscribe(() =>
      this.notificaciones.update((lista) =>
        lista.map((n) => (n.id === notificacion.id ? { ...n, leidaEn: new Date().toISOString() } : n))
      )
    );
  }

  descartarTodas(): void {
    this.noLeidas().forEach((n) => this.descartar(n));
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
