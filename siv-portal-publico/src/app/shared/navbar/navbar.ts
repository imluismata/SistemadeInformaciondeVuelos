import { Component, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../core/services/auth';
import { TemaService } from '../../core/services/tema';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar {
  readonly auth = inject(AuthService);
  readonly tema = inject(TemaService);

  // Controla el menú lateral en pantallas pequeñas (móvil/tablet).
  readonly abierto = signal(false);

  abrir(): void { this.abierto.set(true); }
  cerrar(): void { this.abierto.set(false); }

  cerrarSesion(): void {
    this.auth.cerrarSesion();
    this.cerrar();
  }
}
