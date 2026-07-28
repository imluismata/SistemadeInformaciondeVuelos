import { Injectable, effect, inject, signal } from '@angular/core';
import { AuthService } from './auth';
import { NotificacionesService } from './notificaciones';
import { Notificacion } from '../models/notificacion.model';

const INTERVALO_MS = 15000;   // cada cuánto se consulta a la API
const AUTO_CIERRE_MS = 9000;  // cuánto dura visible cada pop-up

/**
 * Sondea la API periódicamente en busca de notificaciones nuevas para el
 * usuario autenticado. Cuando detecta una que no había visto, la muestra como
 * pop-up (toast) y reproduce un sonido de alerta.
 */
@Injectable({ providedIn: 'root' })
export class NotificacionesPollingService {
  private readonly auth = inject(AuthService);
  private readonly notificaciones = inject(NotificacionesService);

  /** Toasts visibles en pantalla en este momento. */
  readonly toasts = signal<Notificacion[]>([]);

  private readonly vistas = new Set<string>();
  private timer: ReturnType<typeof setInterval> | null = null;
  private primeraPasada = true;

  constructor() {
    // Arranca/detiene el sondeo según el estado de sesión.
    effect(() => {
      const usuario = this.auth.usuarioActual();
      if (usuario) this.iniciar(usuario.id);
      else this.detener();
    });
  }

  private iniciar(usuarioId: string): void {
    this.detener();
    this.primeraPasada = true;
    this.vistas.clear();
    this.sondear(usuarioId);
    this.timer = setInterval(() => this.sondear(usuarioId), INTERVALO_MS);
  }

  private detener(): void {
    if (this.timer) { clearInterval(this.timer); this.timer = null; }
    this.toasts.set([]);
  }

  private sondear(usuarioId: string): void {
    this.notificaciones.obtenerPorUsuario(usuarioId).subscribe({
      next: (todas) => {
        const noLeidas = todas.filter((n) => n.leidaEn === null);
        const nuevas = noLeidas.filter((n) => !this.vistas.has(n.id));
        noLeidas.forEach((n) => this.vistas.add(n.id));

        // La primera pasada solo establece la línea base: no queremos que al
        // iniciar sesión salten en cascada las notificaciones viejas.
        if (this.primeraPasada) { this.primeraPasada = false; return; }

        if (nuevas.length > 0) {
          this.reproducirSonido();
          this.toasts.update((lista) => [...nuevas, ...lista]);
          nuevas.forEach((n) => setTimeout(() => this.descartar(n.id), AUTO_CIERRE_MS));
        }
      },
      error: () => {},
    });
  }

  descartar(id: string): void {
    this.toasts.update((lista) => lista.filter((t) => t.id !== id));
  }

  /** Genera un "ding" de dos tonos con Web Audio API (sin archivo de audio). */
  private reproducirSonido(): void {
    try {
      const Ctx = (window as unknown as {
        AudioContext: typeof AudioContext;
        webkitAudioContext: typeof AudioContext;
      });
      const ctx = new (Ctx.AudioContext || Ctx.webkitAudioContext)();
      const osc = ctx.createOscillator();
      const gain = ctx.createGain();
      osc.connect(gain);
      gain.connect(ctx.destination);

      osc.type = 'sine';
      osc.frequency.setValueAtTime(880, ctx.currentTime);          // La5
      osc.frequency.setValueAtTime(1318.5, ctx.currentTime + 0.15); // Mi6

      gain.gain.setValueAtTime(0.0001, ctx.currentTime);
      gain.gain.exponentialRampToValueAtTime(0.3, ctx.currentTime + 0.02);
      gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + 0.45);

      osc.start();
      osc.stop(ctx.currentTime + 0.45);
      osc.onended = () => ctx.close();
    } catch {
      // Si el navegador bloquea el audio, no interrumpimos la app.
    }
  }
}
