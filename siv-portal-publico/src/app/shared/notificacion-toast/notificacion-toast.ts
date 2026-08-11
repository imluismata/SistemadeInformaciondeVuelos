import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NotificacionesPollingService } from '../../core/services/notificaciones-polling';

/**
 * Pop-up de avisos. Además de pintarlos, decide cuándo hay que buscarlos: el
 * sondeo se enciende mientras este componente está en pantalla y se apaga
 * cuando desaparece.
 *
 * Importa porque las pantallas de la terminal (el tablero, la puerta de
 * embarque...) no montan este componente a propósito — un pop-up tapando el
 * tablero no tendría sentido — y antes seguían preguntando a la API cada 15
 * segundos para descartar la respuesta.
 */
@Component({
  selector: 'app-notificacion-toast',
  imports: [CommonModule, RouterLink],
  templateUrl: './notificacion-toast.html',
  styleUrl: './notificacion-toast.scss',
})
export class NotificacionToast implements OnInit, OnDestroy {
  readonly polling = inject(NotificacionesPollingService);

  ngOnInit(): void {
    this.polling.activar();
  }

  ngOnDestroy(): void {
    this.polling.desactivar();
  }
}
