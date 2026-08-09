import { DatePipe } from '@angular/common';
import { Component, OnDestroy, OnInit, input, signal } from '@angular/core';

/**
 * Marco común de todas las pantallas del aeropuerto: la barra de marca con el
 * reloj arriba y la franja de aviso de seguridad abajo.
 *
 * Está aparte porque el encabezado y el pie se repiten idénticos en todos los
 * diseños (salidas, llegadas, embarque, señalización, directorio, e-ticket).
 * Tenerlo una sola vez evita que dentro de un mes una pantalla muestre una hora
 * y otra no, o que el aviso de seguridad diga cosas distintas según la pantalla.
 * Cada pantalla solo pone su contenido dentro.
 */
@Component({
  selector: 'app-pantalla-marco',
  imports: [DatePipe],
  templateUrl: './pantalla-marco.html',
  styleUrl: './pantalla-marco.scss',
})
export class PantallaMarco implements OnInit, OnDestroy {
  /** Rótulo grande de la pantalla, en español. */
  readonly titulo = input<string>('');
  /** El mismo rótulo en inglés, como en las pantallas reales. */
  readonly tituloEn = input<string>('');

  readonly reloj = signal(new Date());

  private temporizador: ReturnType<typeof setInterval> | null = null;

  ngOnInit(): void {
    this.temporizador = setInterval(() => this.reloj.set(new Date()), 1000);
  }

  ngOnDestroy(): void {
    if (this.temporizador) clearInterval(this.temporizador);
  }
}
