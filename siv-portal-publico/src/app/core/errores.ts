import { HttpErrorResponse } from '@angular/common/http';

/**
 * Saca un mensaje legible de un error de la API.
 *
 * El backend responde los fallos de dos formas distintas y el portal tiene que
 * entender ambas:
 *  - Validación de formato: `[ApiController]` devuelve un ValidationProblemDetails
 *    con los mensajes agrupados por campo dentro de `errors`.
 *  - Reglas de negocio: `ExceptionMiddleware` devuelve `{ error: "mensaje" }`.
 *  - Algunos controladores devuelven directamente un texto plano.
 *
 * Si no se reconoce nada legible se usa el texto por defecto, para no enseñarle
 * al usuario un objeto o un "[object Object]".
 */
export function mensajeDeError(error: unknown, porDefecto: string): string {
  const cuerpo = (error as HttpErrorResponse)?.error;

  if (typeof cuerpo === 'string' && cuerpo.trim())
    return cuerpo;

  if (cuerpo && typeof cuerpo === 'object') {
    const propio = (cuerpo as { error?: unknown }).error;
    if (typeof propio === 'string' && propio.trim())
      return propio;

    const errores = (cuerpo as { errors?: Record<string, string[]> }).errors;
    if (errores) {
      const primero = Object.values(errores).flat().find((m) => typeof m === 'string' && m.trim());
      if (primero) return primero;
    }
  }

  return porDefecto;
}
