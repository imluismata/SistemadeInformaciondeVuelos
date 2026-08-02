import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

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
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

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

      if (destino !== null) {
        router.navigate(['/error', destino]);
      }

      return throwError(() => error);
    }),
  );
};
