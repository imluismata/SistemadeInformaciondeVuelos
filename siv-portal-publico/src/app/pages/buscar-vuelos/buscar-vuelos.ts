import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { VuelosService } from '../../core/services/vuelos';
import { VueloPublico } from '../../core/models/vuelo.model';

type Vista = 'todos' | 'salidas' | 'llegadas';

export const ESTADOS: { valor: string; etiqueta: string }[] = [
  { valor: 'Programado', etiqueta: 'Programado' },
  { valor: 'Retrasado',  etiqueta: 'Retrasado' },
  { valor: 'Embarcando', etiqueta: 'Embarcando' },
  { valor: 'EnVuelo',    etiqueta: 'En vuelo' },
  { valor: 'Aterrizado', etiqueta: 'Aterrizado' },
  { valor: 'Completado', etiqueta: 'Completado' },
  { valor: 'Cancelado',  etiqueta: 'Cancelado' },
];

@Component({
  selector: 'app-buscar-vuelos',
  imports: [CommonModule, FormsModule, RouterLink],
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
    return estado ? lista.filter((v) => v.estado === estado) : lista;
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

  getBadgeClass(estado: string): string {
    const map: Record<string, string> = {
      'Programado': 'on-time', 'EnVuelo': 'boarding', 'Aterrizado': 'arrived',
      'Retrasado': 'delayed', 'Cancelado': 'cancelled', 'Embarcando': 'boarding',
      'Completado': 'departed',
    };
    return map[estado] ?? 'default';
  }
}
