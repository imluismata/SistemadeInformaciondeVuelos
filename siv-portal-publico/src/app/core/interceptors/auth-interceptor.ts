import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { AuthService } from '../services/auth';

/**
 * Adjunta el token JWT en la cabecera Authorization de cada petición cuando hay
 * una sesión activa. Las consultas públicas (sin sesión) salen sin cabecera.
 */
export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const token = inject(AuthService).obtenerToken();

  if (!token) {
    return next(req);
  }

  const autorizada = req.clone({
    setHeaders: { Authorization: `Bearer ${token}` },
  });

  return next(autorizada);
};
