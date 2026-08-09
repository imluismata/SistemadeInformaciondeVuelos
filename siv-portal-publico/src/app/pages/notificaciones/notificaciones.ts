import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../core/services/auth';
import { DispositivoService } from '../../core/services/dispositivo';
import { NotificacionesService } from '../../core/services/notificaciones';
import { Notificacion } from '../../core/models/notificacion.model';

/**
 * Historial completo de notificaciones del visitante, tenga cuenta o no.
 *
 * Cada aviso lleva al vuelo que cambió, igual que el pop-up: son la misma
 * notificación vista de dos formas — el pop-up es el aviso del momento, esta
 * página es el registro de todo lo que ha pasado.
 */
@Component({
  selector: 'app-notificaciones',
  imports: [CommonModule, RouterLink],
  templateUrl: './notificaciones.html',
  styleUrl: './notificaciones.scss',
})
export class Notificaciones implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly dispositivo = inject(DispositivoService);
  private readonly servicio = inject(NotificacionesService);
  private readonly router = inject(Router);

  readonly cargando = signal(true);
  readonly notificaciones = signal<Notificacion[]>([]);

  readonly sinIdentidad = computed(
    () => !this.auth.estaAutenticado() && !this.dispositivo.id()
  );

  readonly noLeidas = computed(
    () => this.notificaciones().filter((n) => n.leidaEn === null).length
  );

  ngOnInit(): void {
    const usuario = this.auth.usuarioActual();
    const dispositivoId = this.dispositivo.id();

    const consulta = usuario
      ? this.servicio.obtenerPorUsuario(usuario.id)
      : dispositivoId
        ? this.servicio.obtenerPorDispositivo(dispositivoId)
        : null;

    if (!consulta) { this.cargando.set(false); return; }

    consulta.subscribe({
      // De la más vieja a la más reciente: se leen como una línea de tiempo del
      // vuelo, en el orden en que fueron pasando las cosas.
      next: (lista) => {
        this.notificaciones.set(
          [...lista].sort((a, b) => +new Date(a.generadaEn) - +new Date(b.generadaEn))
        );
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false),
    });
  }

  /** Abre el vuelo que cambió y, de paso, da la notificación por leída. */
  abrir(notificacion: Notificacion): void {
    this.marcarLeida(notificacion);
    this.router.navigate(['/vuelos', notificacion.vueloId], {
      state: { mensajeCambio: notificacion.mensaje },
    });
  }

  marcarTodas(): void {
    this.notificaciones()
      .filter((n) => n.leidaEn === null)
      .forEach((n) => this.marcarLeida(n));
  }

  private marcarLeida(notificacion: Notificacion): void {
    if (notificacion.leidaEn !== null) return;

    const dispositivoId = this.dispositivo.id();
    const peticion = this.auth.estaAutenticado() || !dispositivoId
      ? this.servicio.marcarComoLeida(notificacion.id)
      : this.servicio.marcarComoLeidaAnonima(notificacion.id, dispositivoId);

    peticion.subscribe({
      next: () => this.notificaciones.update((lista) =>
        lista.map((n) =>
          n.id === notificacion.id ? { ...n, leidaEn: new Date().toISOString() } : n
        )
      ),
      error: () => {},
    });
  }
}
