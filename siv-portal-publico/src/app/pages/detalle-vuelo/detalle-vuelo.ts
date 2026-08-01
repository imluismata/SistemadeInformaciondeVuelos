import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { VuelosService } from '../../core/services/vuelos';
import { SeguimientoService } from '../../core/services/seguimiento';
import { AuthService } from '../../core/services/auth';
import { VueloPublico } from '../../core/models/vuelo.model';

@Component({
  selector: 'app-detalle-vuelo',
  imports: [CommonModule, RouterLink],
  templateUrl: './detalle-vuelo.html',
  styleUrl: './detalle-vuelo.scss',
})
export class DetalleVuelo implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly vuelosService = inject(VuelosService);
  private readonly seguimientoService = inject(SeguimientoService);
  readonly auth = inject(AuthService);

  readonly vuelo = signal<VueloPublico | null>(null);
  readonly cargando = signal(true);
  readonly mensaje = signal<string | null>(null);
  readonly mensajeEsError = signal(false);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.vuelosService.obtenerDetalle(id).subscribe({
      next: (vuelo) => { this.vuelo.set(vuelo); this.cargando.set(false); },
      error: () => this.cargando.set(false),
    });
  }

  seguirVuelo(): void {
    const usuario = this.auth.usuarioActual();
    const vuelo = this.vuelo();
    if (!usuario || !vuelo) return;

    this.seguimientoService
      .registrar({ usuarioId: usuario.id, vueloId: vuelo.id })
      .subscribe({
        next: () => {
          this.mensajeEsError.set(false);
          this.mensaje.set('Ahora sigues este vuelo. Recibirás notificaciones de cambios.');
        },
        error: (e) => {
          // 409 = el backend indica que ya existe un seguimiento activo para este vuelo.
          if (e?.status === 409) {
            this.mensajeEsError.set(false);
            this.mensaje.set('Ya estás siguiendo este vuelo.');
          } else {
            this.mensajeEsError.set(true);
            this.mensaje.set('No se pudo registrar el seguimiento.');
          }
        },
      });
  }

  getBadge(estado: string): string {
    const map: Record<string, string> = {
      'Programado': 'on-time', 'EnVuelo': 'boarding', 'Aterrizado': 'arrived',
      'Retrasado': 'delayed', 'Cancelado': 'cancelled', 'Embarcando': 'boarding',
      'Completado': 'departed',
    };
    return map[estado] ?? 'default';
  }
}
