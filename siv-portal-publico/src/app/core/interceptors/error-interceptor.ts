import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { PETICION_DE_FONDO } from '../peticion-de-fondo';

/**
 * Ante fallos de la API que no tienen manejo específico en pantalla, lleva al
 * usuario a una página de error con la marca (caso "API no disponible" y afines
 * de la práctica de presentación).
 *
 * - status 0  → red caída / API no responde → 503
 * - 502/503/504 → servicio no disponible → 503
 * - 500 → error interno
 * - 403 → acceso denegado
 * 401/404/400 se dejan pasar: el 401 lo maneja el flujo de login y el 404/400 se
 * muestran en contexto (p. ej. "vuelo no encontrado"), no como página completa.
 *
 * Las peticiones marcadas como de fondo (el sondeo de notificaciones) nunca
 * navegan: sacar al usuario de donde está por un sondeo que él no pidió sería
 * peor que el propio fallo. El error igual se propaga, y quien llamó decide.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const esDeFondo = req.context.get(PETICION_DE_FONDO);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let destino: number | null = null;

      if (error.status === 0 || error.status === 502 || error.status === 503 || error.status === 504) {
        destino = 503;
      } else if (error.status === 500) {
        destino = 500;
      } else if (error.status === 403) {
        destino = 403;
      }

      if (destino !== null && !esDeFondo) {
        router.navigate(['/error', destino]);
      }

      return throwError(() => error);
    }),
  );
};
