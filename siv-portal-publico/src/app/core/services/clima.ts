import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, catchError, map, of } from 'rxjs';
import { contextoDeFondo } from '../peticion-de-fondo';

export interface Clima {
  temperatura: number;
  descripcion: string;
  /** Familia del tiempo, para elegir el dibujo: sol, nubes o lluvia. */
  icono: 'sol' | 'nubes' | 'lluvia';
}

interface RespuestaOpenMeteo {
  current?: { temperature_2m?: number; weather_code?: number };
}

/**
 * Clima del aeropuerto de destino, para la pantalla de puerta de embarque.
 *
 * Usa Open-Meteo: es gratuito, no pide registro ni clave de API y responde
 * directo al navegador. Por eso no hace falta tocar el backend — se eligió
 * precisamente para no meter una clave secreta en el portal, que es lo que
 * obligaría a hacer un proxy en la API.
 *
 * El clima es un adorno: si el servicio no responde, la pantalla se queda sin
 * ese recuadro y el vuelo se sigue viendo. Por eso nunca propaga el error.
 */
@Injectable({ providedIn: 'root' })
export class ClimaService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'https://api.open-meteo.com/v1/forecast';

  obtener(latitud: number, longitud: number): Observable<Clima | null> {
    const url = `${this.baseUrl}?latitude=${latitud}&longitude=${longitud}`
      + '&current=temperature_2m,weather_code';

    // De fondo: que un fallo del clima no arrastre la pantalla a la página de error.
    return this.http.get<RespuestaOpenMeteo>(url, { context: contextoDeFondo() }).pipe(
      map((r) => {
        const temperatura = r.current?.temperature_2m;
        if (temperatura === undefined) return null;

        const { descripcion, icono } = interpretar(r.current?.weather_code ?? 0);
        return { temperatura: Math.round(temperatura), descripcion, icono };
      }),
      catchError(() => of(null)),
    );
  }
}

/**
 * Traduce el código WMO que devuelve Open-Meteo. Se agrupan en pocas familias a
 * propósito: en una pantalla que se lee de lejos, "llovizna ligera intermitente"
 * no aporta nada sobre "lluvia".
 */
function interpretar(codigo: number): { descripcion: string; icono: Clima['icono'] } {
  if (codigo === 0) return { descripcion: 'Despejado', icono: 'sol' };
  if (codigo <= 2) return { descripcion: 'Parcialmente nublado', icono: 'nubes' };
  if (codigo === 3) return { descripcion: 'Nublado', icono: 'nubes' };
  if (codigo <= 48) return { descripcion: 'Neblina', icono: 'nubes' };
  if (codigo <= 67) return { descripcion: 'Lluvia', icono: 'lluvia' };
  if (codigo <= 77) return { descripcion: 'Nieve', icono: 'lluvia' };
  if (codigo <= 82) return { descripcion: 'Chubascos', icono: 'lluvia' };
  return { descripcion: 'Tormenta', icono: 'lluvia' };
}
