import { Component, computed, input, signal } from '@angular/core';

/** Color de marca de cada aerolínea, para el recuadro del código. */
const COLORES: Record<string, string> = {
  AA: '#0078d2', // American Airlines
  B6: '#003876', // JetBlue
  CM: '#0033a0', // Copa Airlines
  DM: '#652d86', // Arajet
  AV: '#da291c', // Avianca
  LA: '#1b0088', // LATAM
  AM: '#0b2265', // Aeroméxico
  IB: '#d7192d', // Iberia
  AF: '#002157', // Air France
};

const COLOR_POR_DEFECTO = '#334155';

/**
 * Formatos que se prueban, en orden. Se aceptan los dos porque los logos se
 * descargan de sitios distintos y unos vienen vectoriales y otros en mapa de
 * bits; así no hay que convertir nada antes de dejarlos en la carpeta.
 */
const FORMATOS = ['png', 'svg'] as const;

/**
 * Identificación visual de la aerolínea, como en las pantallas de aeropuerto.
 *
 * Funciona en dos niveles: si existe el archivo public/aerolineas/{CODIGO}.png
 * (o .svg) lo usa; si no, dibuja un recuadro con el código en el color de la
 * marca. Así la pantalla se ve bien desde el primer día y basta con dejar caer
 * los logos en esa carpeta para que aparezcan, sin tocar una línea de código.
 *
 * El código (AA, B6, DM...) viene de la API, no se adivina del nombre.
 */
@Component({
  selector: 'app-logo-aerolinea',
  imports: [],
  templateUrl: './logo-aerolinea.html',
  styleUrl: './logo-aerolinea.scss',
})
export class LogoAerolinea {
  readonly codigo = input.required<string>();
  /** Nombre completo, para lectores de pantalla. */
  readonly nombre = input<string>('');

  /** Se apaga cuando se agotan los formatos sin encontrar imagen. */
  readonly hayImagen = signal(true);

  private readonly formato = signal(0);

  protected readonly clave = computed(() => (this.codigo() || '').toUpperCase());
  protected readonly ruta = computed(
    () => `aerolineas/${this.clave()}.${FORMATOS[this.formato()]}`
  );
  protected readonly color = computed(() => COLORES[this.clave()] ?? COLOR_POR_DEFECTO);

  /** Si un formato no está, se prueba el siguiente; al agotarlos, va el recuadro. */
  protected alFallar(): void {
    if (this.formato() < FORMATOS.length - 1) this.formato.update((i) => i + 1);
    else this.hayImagen.set(false);
  }
}
