import { Component, OnInit, inject, signal } from '@angular/core';
import { VuelosService } from '../../core/services/vuelos';

interface Pantalla {
  titulo: string;
  descripcion: string;
  ruta: string;
}

/**
 * Panel de lanzamiento de las pantallas del aeropuerto.
 *
 * Las pantallas viven dentro de este mismo portal y no en una aplicación
 * aparte: muestran la misma información pública y anónima que ya sirve el
 * portal, así que separarlas obligaría a duplicar modelos, servicios y estilos
 * sin ganar ningún aislamiento a cambio.
 *
 * Esta página sí es del portal (lleva menú); las pantallas que abre van a
 * pantalla completa y sin menú, porque una pantalla colgada en la terminal no
 * se navega.
 */
@Component({
  selector: 'app-pantallas',
  imports: [],
  templateUrl: './pantallas.html',
  styleUrl: './pantallas.scss',
})
export class Pantallas implements OnInit {
  private readonly vuelosService = inject(VuelosService);

  readonly tableros: Pantalla[] = [
    {
      titulo: 'Tablero mixto',
      descripcion: 'Alterna salidas y llegadas cada 15 segundos. Para zonas de paso.',
      ruta: '/tablero',
    },
    {
      titulo: 'Salidas',
      descripcion: 'Solo salidas, con hora de embarque y puerta. Para el área de facturación.',
      ruta: '/tablero/salidas',
    },
    {
      titulo: 'Llegadas',
      descripcion: 'Solo llegadas, por hora de aterrizaje. Para el vestíbulo de recibimiento.',
      ruta: '/tablero/llegadas',
    },
  ];

  readonly informativas: Pantalla[] = [
    {
      titulo: 'E-Ticket',
      descripcion: 'Instrucciones y código QR del formulario de Migración.',
      ruta: '/tablero/eticket',
    },
    {
      titulo: 'Señalización',
      descripcion: 'Carteles direccionales de pasillo: puertas, equipaje y servicios.',
      ruta: '/tablero/senalizacion',
    },
    {
      titulo: 'Directorio',
      descripcion: 'Plano de la terminal, leyenda y servicios disponibles.',
      ruta: '/tablero/directorio',
    },
  ];

  /** Puertas con vuelo asignado hoy, para no escribir la dirección a mano. */
  readonly puertas = signal<string[]>([]);
  readonly cargandoPuertas = signal(true);

  ngOnInit(): void {
    this.vuelosService.obtenerSalidas().subscribe({
      next: (vuelos) => {
        const enUso = vuelos
          .map((v) => v.puerta)
          .filter((p): p is string => !!p);

        this.puertas.set([...new Set(enUso)].sort());
        this.cargandoPuertas.set(false);
      },
      error: () => this.cargandoPuertas.set(false),
    });
  }
}
