import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { VuelosService } from '../../core/services/vuelos';
import { VueloPublico } from '../../core/models/vuelo.model';
import { nombreCorto } from '../../core/aeropuertos';
import { EstadoVuelo } from '../../shared/estado-vuelo/estado-vuelo';
import { LogoAerolinea } from '../../shared/logo-aerolinea/logo-aerolinea';
import { PantallaMarco } from '../../shared/pantalla-marco/pantalla-marco';

/** Cada cuánto se vuelven a pedir los vuelos a la API. */
const REFRESCO_DATOS_MS = 30000;
/** Cada cuánto alterna la pantalla mixta entre salidas y llegadas. */
const CAMBIO_VISTA_MS = 15000;

/**
 * Minutos antes de la salida a los que empieza el embarque.
 *
 * Es una regla, no un dato: en vez de agregarle un campo a cada vuelo y pedirle
 * al operador que lo llene uno por uno, se calcula. Si el aeropuerto cambia su
 * política, se cambia este número y todas las pantallas obedecen.
 */
const MINUTOS_ANTES_DE_EMBARQUE = 45;

type Vista = 'salidas' | 'llegadas';
/** "mixto" es la pantalla de sala que va alternando; las otras son fijas. */
type Modo = 'mixto' | Vista;

/**
 * Tablero público de vuelos (FIDS). La misma pantalla sirve tres sitios
 * distintos según la ruta:
 *
 *  /tablero          → alterna salidas y llegadas (zonas de paso)
 *  /tablero/salidas  → solo salidas (mostradores, pasillo de embarque)
 *  /tablero/llegadas → solo llegadas (vestíbulo donde se espera a la gente)
 *
 * Es un componente y no tres porque el contenido es el mismo: lo único que
 * cambia es el filtro. Tres copias serían tres sitios donde arreglar el mismo
 * error.
 *
 * No hay endpoints nuevos: reutiliza la consulta pública que ya existía. Lo que
 * cambia es el modo de consumo — nadie va a pulsar "actualizar" en una pantalla
 * colgada del techo, así que se refresca sola.
 */
@Component({
  selector: 'app-tablero',
  imports: [CommonModule, EstadoVuelo, LogoAerolinea, PantallaMarco],
  templateUrl: './tablero.html',
  styleUrl: './tablero.scss',
})
export class Tablero implements OnInit, OnDestroy {
  private readonly vuelosService = inject(VuelosService);
  private readonly ruta = inject(ActivatedRoute);

  readonly modo = signal<Modo>('mixto');
  readonly vista = signal<Vista>('salidas');
  readonly cargando = signal(true);
  readonly sinConexion = signal(false);

  private readonly salidas = signal<VueloPublico[]>([]);
  private readonly llegadas = signal<VueloPublico[]>([]);

  readonly esSalidas = computed(() => this.vista() === 'salidas');

  /**
   * Los vuelos de la vista actual, siempre por hora y el más próximo arriba: en
   * una pantalla de aeropuerto el orden cronológico no es un lujo, es la única
   * forma de encontrar tu vuelo de un vistazo.
   */
  readonly vuelos = computed(() => {
    const esSalidas = this.esSalidas();
    const lista = esSalidas ? this.salidas() : this.llegadas();

    return [...lista].sort((a, b) =>
      +new Date(this.horaDeLaVista(a, esSalidas)) - +new Date(this.horaDeLaVista(b, esSalidas))
    );
  });

  private temporizadores: ReturnType<typeof setInterval>[] = [];

  ngOnInit(): void {
    const modo = (this.ruta.snapshot.data['modo'] as Modo) ?? 'mixto';
    this.modo.set(modo);
    this.vista.set(modo === 'llegadas' ? 'llegadas' : 'salidas');

    this.cargar();
    this.temporizadores.push(setInterval(() => this.cargar(), REFRESCO_DATOS_MS));

    // Solo la pantalla mixta alterna; las fijas se quedan en lo suyo.
    if (modo === 'mixto')
      this.temporizadores.push(setInterval(() => this.alternarVista(), CAMBIO_VISTA_MS));
  }

  ngOnDestroy(): void {
    // Sin esto los temporizadores seguirían vivos al salir de la pantalla.
    this.temporizadores.forEach(clearInterval);
  }

  /** Hora de embarque: no viene de la API, se deduce de la hora de salida. */
  horaDeEmbarque(vuelo: VueloPublico): Date {
    const salida = new Date(vuelo.horarioSalida);
    return new Date(salida.getTime() - MINUTOS_ANTES_DE_EMBARQUE * 60000);
  }

  /**
   * Ciudad del otro extremo del vuelo: el destino en salidas, el origen en
   * llegadas. Se prefiere la ciudad ("Miami") al nombre completo del aeropuerto,
   * que en una pantalla ocupa una línea entera y dice menos.
   */
  ciudad(vuelo: VueloPublico): string {
    return this.esSalidas()
      ? nombreCorto(vuelo.destinoCodigo, vuelo.destino)
      : nombreCorto(vuelo.origenCodigo, vuelo.origen);
  }

  /** Hora que manda en esta vista: la de salida o la de llegada. */
  hora(vuelo: VueloPublico): string {
    return this.horaDeLaVista(vuelo, this.esSalidas());
  }

  /**
   * Hora originalmente programada, solo si el vuelo se movió. Es la que se pinta
   * tachada al lado de la nueva.
   */
  horaOriginal(vuelo: VueloPublico): string | null {
    const original = this.esSalidas() ? vuelo.horarioSalidaOriginal : vuelo.horarioLlegadaOriginal;
    if (!original) return null;

    // Si coincide con la actual, el vuelo no se movió: no hay nada que tachar.
    return original === this.hora(vuelo) ? null : original;
  }

  private horaDeLaVista(vuelo: VueloPublico, esSalidas: boolean): string {
    return esSalidas ? vuelo.horarioSalida : vuelo.horarioLlegada;
  }

  private alternarVista(): void {
    this.vista.update((v) => (v === 'salidas' ? 'llegadas' : 'salidas'));
  }

  private cargar(): void {
    this.vuelosService.obtenerSalidas().subscribe({
      next: (vuelos) => this.recibir(() => this.salidas.set(vuelos)),
      // Si la API falla no se borra lo que ya se mostraba: en una pantalla de
      // aeropuerto es preferible información de hace un minuto que una pantalla
      // en blanco. Solo se avisa que los datos no están frescos.
      error: () => { this.cargando.set(false); this.sinConexion.set(true); },
    });

    this.vuelosService.obtenerLlegadas().subscribe({
      next: (vuelos) => this.recibir(() => this.llegadas.set(vuelos)),
      error: () => { this.cargando.set(false); this.sinConexion.set(true); },
    });
  }

  private recibir(guardar: () => void): void {
    guardar();
    this.cargando.set(false);
    this.sinConexion.set(false);
  }
}
