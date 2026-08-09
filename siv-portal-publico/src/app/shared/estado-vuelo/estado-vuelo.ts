import { Component, computed, input } from '@angular/core';
import { etiquetaEstado } from '../../core/estados-vuelo';

/**
 * Badge con el estado del vuelo tal como se ve en una pantalla de aeropuerto:
 * el rótulo en español y debajo el equivalente en inglés.
 *
 * Está como componente (y no repetido en cada plantilla) para que el listado,
 * el detalle y cualquier pantalla futura muestren el estado exactamente igual.
 */
@Component({
  selector: 'app-estado-vuelo',
  templateUrl: './estado-vuelo.html',
  styleUrl: './estado-vuelo.scss',
})
export class EstadoVuelo {
  /** Estado del dominio tal como viene de la API: "Programado", "Retrasado"... */
  readonly estado = input.required<string>();

  /**
   * "chip" es el badge redondeado del portal; "pantalla" es el rótulo de la
   * pantalla de aeropuerto: sin fondo, letra grande y color fuerte, que es como
   * se lee de lejos. El mapa de estados es el mismo en ambos casos.
   */
  readonly variante = input<'chip' | 'pantalla'>('chip');

  protected readonly etiqueta = computed(() => etiquetaEstado(this.estado()));
}
