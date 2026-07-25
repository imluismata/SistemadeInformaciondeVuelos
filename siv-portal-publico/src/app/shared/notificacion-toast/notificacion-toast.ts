import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { NotificacionesPollingService } from '../../core/services/notificaciones-polling';

@Component({
  selector: 'app-notificacion-toast',
  imports: [CommonModule, RouterLink],
  templateUrl: './notificacion-toast.html',
  styleUrl: './notificacion-toast.scss',
})
export class NotificacionToast {
  readonly polling = inject(NotificacionesPollingService);
}
