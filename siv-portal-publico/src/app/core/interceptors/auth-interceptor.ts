import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth';

/**
 * Adjunta el token JWT en la cabecera Authorization de cada petición **a nuestra
 * API** cuando hay una sesión activa. Las consultas públicas (sin sesión) salen
 * sin cabecera.
 *
 * El filtro por URL no es un detalle: el portal también llama a servicios de
 * terceros (por ejemplo el clima del destino en la pantalla de embarque), y sin
 * esta comprobación el token del usuario viajaría a un servidor ajeno. Un token
 * solo debe presentarse ante quien lo emitió.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const esNuestraApi = req.url.startsWith(environment.apiUrl);
  const token = inject(AuthService).obtenerToken();

  if (!token || !esNuestraApi) {
    return next(req);
  }

  const autorizada = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });

  return next(autorizada);
};
