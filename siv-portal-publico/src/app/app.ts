import { Component, inject, signal } from '@angular/core';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { Navbar } from './shared/navbar/navbar';
import { NotificacionToast } from './shared/notificacion-toast/notificacion-toast';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, Navbar, NotificacionToast],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  /**
   * El tablero de la terminal (/tablero) va sin menú lateral y sin pop-ups:
   * es una pantalla colgada en el aeropuerto, no una página para navegar.
   */
  protected readonly esTablero = signal(false);

  constructor() {
    const router = inject(Router);
    this.esTablero.set(router.url.startsWith('/tablero'));

    // No hace falta desuscribirse: este es el componente raíz, vive lo mismo
    // que la aplicación.
    router.events.subscribe((evento) => {
      if (evento instanceof NavigationEnd)
        this.esTablero.set(evento.urlAfterRedirects.startsWith('/tablero'));
    });
  }
}
