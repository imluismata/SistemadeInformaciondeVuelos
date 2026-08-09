import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PETICION_DE_FONDO } from '../peticion-de-fondo';
import { AuthService } from '../services/auth';

/** El login responde 401 con credenciales malas; ese 401 no es sesión vencida. */
const URL_LOGIN = `${environment.apiUrl}/auth/login`;

/**
 * Ante fallos de la API que no tienen manejo específico en pantalla, lleva al
 * usuario a una página de error con la marca (caso "API no disponible" y afines
 * de la práctica de presentación).
 *
 * - status 401 → el token venció o no vale: se cierra la sesión y se va al login
 * - status 0  → red caída / API no responde → 503
 * - 502/503/504 → servicio no disponible → 503
 * - 500 → error interno
 * - 403 → acceso denegado
 * 404/400 se dejan pasar: se muestran en contexto (p. ej. "vuelo no encontrado"),
 * no como página completa.
 *
 * Las peticiones marcadas como de fondo (el sondeo de notificaciones) nunca
 * navegan: sacar al usuario de donde está por un sondeo que él no pidió sería
 * peor que el propio fallo. El error igual se propaga, y quien llamó decide.
 */
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);
  const auth = inject(AuthService);
  const esDeFondo = req.context.get(PETICION_DE_FONDO);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      // Sesión vencida. El token dura una hora, así que pasa solo: sin esto el
      // portal seguía mostrando al usuario como conectado y cada pantalla
      // enseñaba su propio error genérico, sin decir lo único que importa.
      //
      // Aquí sí se navega aunque la petición sea de fondo, al revés que con los
      // demás errores: un fallo de red es pasajero, pero una sesión vencida no
      // se arregla sola. Como el sondeo corre cada 15 segundos, casi siempre es
      // él quien se entera primero; si no navegara, al usuario se le cerraría la
      // sesión en silencio y descubriría el problema al pulsar un botón que no
      // responde. Solo aplica a quien tenía sesión: al visitante anónimo no se
      // le manda a ningún login.
      if (error.status === 401 && req.url !== URL_LOGIN) {
        if (auth.estaAutenticado()) {
          auth.cerrarSesion();
          router.navigate(['/login'], { queryParams: { expirada: '1' } });
        }

        return throwError(() => error);
      }

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
