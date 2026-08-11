import { CommonModule } from '@angular/common';
import { Component, OnDestroy, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { VuelosService } from '../../core/services/vuelos';
import { ClimaService, Clima } from '../../core/services/clima';
import { VueloPublico } from '../../core/models/vuelo.model';
import { buscarAeropuerto } from '../../core/aeropuertos';
import { EstadoVuelo } from '../../shared/estado-vuelo/estado-vuelo';
import { LogoAerolinea } from '../../shared/logo-aerolinea/logo-aerolinea';
import { PantallaMarco } from '../../shared/pantalla-marco/pantalla-marco';

const REFRESCO_DATOS_MS = 30000;
/** El clima cambia despacio: pedirlo cada 15 minutos es de sobra. */
const REFRESCO_CLIMA_MS = 900000;

/**
 * Pantalla de una puerta de embarque concreta: la que cuelga sobre el mostrador
 * de la puerta. Se abre con la puerta en la dirección, por ejemplo
 * /tablero/embarque/A3, así cada pantalla física apunta a la suya.
 *
 * Muestra el próximo vuelo asignado a esa puerta. No hay endpoint nuevo: se
 * filtra sobre las salidas que ya devuelve la consulta pública.
 */
@Component({
  selector: 'app-embarque',
  imports: [CommonModule, EstadoVuelo, LogoAerolinea, PantallaMarco],
  templateUrl: './embarque.html',
  styleUrl: './embarque.scss',
})
export class Embarque implements OnInit, OnDestroy {
  private readonly vuelosService = inject(VuelosService);
  private readonly climaService = inject(ClimaService);
  private readonly ruta = inject(ActivatedRoute);

  readonly puerta = signal('');
  readonly vuelo = signal<VueloPublico | null>(null);
  readonly clima = signal<Clima | null>(null);
  readonly cargando = signal(true);
  readonly ahora = signal(new Date());

  /** Datos geográficos del destino (ciudad y coordenadas), si lo conocemos. */
  readonly destino = computed(() => buscarAeropuerto(this.vuelo()?.destinoCodigo));

  /**
   * Cuánto falta para la salida, en horas y minutos. Null si el vuelo ya salió
   * o no hay vuelo: en ese caso no se enseña una cuenta atrás en negativo.
   */
  readonly cuentaAtras = computed(() => {
    const vuelo = this.vuelo();
    if (!vuelo) return null;

    const restanMs = +new Date(vuelo.horarioSalida) - +this.ahora();
    if (restanMs <= 0) return null;

    const minutosTotales = Math.floor(restanMs / 60000);
    return {
      horas: String(Math.floor(minutosTotales / 60)).padStart(2, '0'),
      minutos: String(minutosTotales % 60).padStart(2, '0'),
    };
  });

  private temporizadores: ReturnType<typeof setInterval>[] = [];
  /** Código del destino cuyo clima ya se pidió, para no repetir la llamada. */
  private climaDe: string | null = null;

  ngOnInit(): void {
    this.puerta.set((this.ruta.snapshot.paramMap.get('puerta') ?? '').toUpperCase());

    this.cargar();
    this.temporizadores = [
      setInterval(() => this.cargar(), REFRESCO_DATOS_MS),
      setInterval(() => this.ahora.set(new Date()), 1000),
      setInterval(() => { this.climaDe = null; this.actualizarClima(); }, REFRESCO_CLIMA_MS),
    ];
  }

  ngOnDestroy(): void {
    this.temporizadores.forEach(clearInterval);
  }

  private cargar(): void {
    this.vuelosService.obtenerSalidas().subscribe({
      next: (vuelos) => {
        this.vuelo.set(this.vueloDeLaPuerta(vuelos));
        this.cargando.set(false);
        this.actualizarClima();
      },
      // Si la API falla se conserva lo que ya se mostraba: en la puerta de
      // embarque una pantalla en blanco es peor que un dato de hace un minuto.
      error: () => this.cargando.set(false),
    });
  }

  /** De los vuelos asignados a esta puerta, el más próximo a salir. */
  private vueloDeLaPuerta(vuelos: VueloPublico[]): VueloPublico | null {
    const puerta = this.puerta();

    return vuelos
      .filter((v) => (v.puerta ?? '').toUpperCase() === puerta)
      .sort((a, b) => +new Date(a.horarioSalida) - +new Date(b.horarioSalida))[0] ?? null;
  }

  /** Pide el clima solo cuando cambia el destino, no en cada refresco. */
  private actualizarClima(): void {
    const destino = this.destino();
    const codigo = this.vuelo()?.destinoCodigo ?? null;

    if (!destino || !codigo) { this.clima.set(null); this.climaDe = null; return; }
    if (this.climaDe === codigo) return;

    this.climaDe = codigo;
    this.climaService.obtener(destino.latitud, destino.longitud)
      .subscribe((clima) => this.clima.set(clima));
  }
}
