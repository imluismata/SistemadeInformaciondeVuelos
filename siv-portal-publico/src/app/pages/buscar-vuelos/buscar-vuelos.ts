import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { VuelosService } from '../../core/services/vuelos';
import { VueloPublico } from '../../core/models/vuelo.model';

@Component({
  selector: 'app-buscar-vuelos',
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './buscar-vuelos.html',
  styleUrl: './buscar-vuelos.scss',
})
export class BuscarVuelos implements OnInit {
  private readonly vuelosService = inject(VuelosService);

  readonly vuelos = signal<VueloPublico[]>([]);
  readonly cargando = signal(false);
  readonly error = signal<string | null>(null);

  origen = '';
  destino = '';
  fecha = '';

  ngOnInit(): void {
    this.cargarActivos();
  }

  cargarActivos(): void {
    this.cargando.set(true);
    this.error.set(null);

    this.vuelosService.obtenerActivos().subscribe({
      next: (vuelos) => {
        this.vuelos.set(vuelos);
        this.cargando.set(false);
      },
      error: () => {
        this.error.set('No se pudieron cargar los vuelos. Intenta de nuevo.');
        this.cargando.set(false);
      },
    });
  }

  buscar(): void {
    this.cargando.set(true);
    this.error.set(null);

    this.vuelosService
      .buscarConFiltro({
        origen: this.origen || null,
        destino: this.destino || null,
        fecha: this.fecha || null,
      })
      .subscribe({
        next: (vuelos) => {
          this.vuelos.set(vuelos);
          this.cargando.set(false);
        },
        error: () => {
          this.error.set('No se pudo realizar la búsqueda.');
          this.cargando.set(false);
        },
      });
  }
}
