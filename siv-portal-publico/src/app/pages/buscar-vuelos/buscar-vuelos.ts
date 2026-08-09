import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { VuelosService } from '../../core/services/vuelos';
import { VueloPublico } from '../../core/models/vuelo.model';
import { ESTADOS_VUELO, etiquetaEstado } from '../../core/estados-vuelo';
import { EstadoVuelo } from '../../shared/estado-vuelo/estado-vuelo';
import { LogoAerolinea } from '../../shared/logo-aerolinea/logo-aerolinea';

type Vista = 'todos' | 'salidas' | 'llegadas';

// Los chips filtran por el estado real del dominio, pero se rotulan igual que
// el badge (así el que filtra por "A tiempo" ve vuelos con badge "A tiempo").
export const ESTADOS = ESTADOS_VUELO.map((valor) => ({
  valor,
  etiqueta: etiquetaEstado(valor).es,
}));

@Component({
  selector: 'app-buscar-vuelos',
  imports: [CommonModule, FormsModule, RouterLink, EstadoVuelo, LogoAerolinea],
  templateUrl: './buscar-vuelos.html',
  styleUrl: './buscar-vuelos.scss',
})
export class BuscarVuelos implements OnInit {
  private readonly vuelosService = inject(VuelosService);

  readonly estados = ESTADOS;

  readonly vuelos = signal<VueloPublico[]>([]);
  readonly cargando = signal(false);
  readonly error = signal<string | null>(null);
  readonly vista = signal<Vista>('todos');
  readonly estadoFiltro = signal<string>('');

  readonly vuelosFiltrados = computed(() => {
    const estado = this.estadoFiltro();
    const lista = this.vuelos();
    const filtrados = estado ? lista.filter((v) => v.estado === estado) : lista;

    // Siempre por hora, el más temprano primero, como cualquier tablero de
    // aeropuerto. En las llegadas manda la hora de llegada, no la de salida:
    // al pasajero que espera a alguien le importa cuándo aterriza.
    // La API ya los devuelve ordenados, pero se ordena también aquí para que el
    // orden no dependa del endpoint que se haya usado (búsqueda, filtro...).
    const porHora = (v: VueloPublico) =>
      +new Date(this.vista() === 'llegadas' ? v.horarioLlegada : v.horarioSalida);

    return [...filtrados].sort((a, b) => porHora(a) - porHora(b));
  });

  numero = '';
  origen = '';
  destino = '';
  fecha = '';

  ngOnInit(): void {
    this.cargarVista();
  }

  setVista(v: Vista): void {
    this.vista.set(v);
    this.estadoFiltro.set('');
    this.numero = '';
    this.origen = '';
    this.destino = '';
    this.fecha = '';
    this.cargarVista();
  }

  filtrarPorEstado(estado: string): void {
    this.estadoFiltro.set(this.estadoFiltro() === estado ? '' : estado);
  }

  cargarVista(): void {
    this.cargando.set(true);
    this.error.set(null);

    const obs$ = this.vista() === 'salidas'
      ? this.vuelosService.obtenerSalidas()
      : this.vista() === 'llegadas'
        ? this.vuelosService.obtenerLlegadas()
        : this.vuelosService.obtenerActivos();

    obs$.subscribe({
      next: (vuelos) => { this.vuelos.set(vuelos); this.cargando.set(false); },
      error: () => { this.error.set('No se pudieron cargar los vuelos.'); this.cargando.set(false); },
    });
  }

  buscar(): void {
    if (this.numero.trim()) {
      this.cargando.set(true);
      this.error.set(null);
      this.vuelosService.buscarPorNumero(this.numero.trim()).subscribe({
        next: (vuelo) => { this.vuelos.set([vuelo]); this.cargando.set(false); },
        error: () => { this.vuelos.set([]); this.error.set('No se encontró el vuelo.'); this.cargando.set(false); },
      });
      return;
    }

    this.cargando.set(true);
    this.error.set(null);
    this.vuelosService.buscarConFiltro({
      origen: this.origen || null,
      destino: this.destino || null,
      fecha: this.fecha || null,
    }).subscribe({
      next: (vuelos) => { this.vuelos.set(vuelos); this.cargando.set(false); },
      error: () => { this.error.set('No se pudo realizar la búsqueda.'); this.cargando.set(false); },
    });
  }
}
