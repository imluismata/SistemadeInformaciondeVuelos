import { HttpContext, HttpContextToken } from '@angular/common/http';

/**
 * Marca una petición como "de fondo": la hace la aplicación por su cuenta, no
 * porque el usuario haya pedido algo.
 *
 * Sirve para que el interceptor de errores no lo saque de la página en la que
 * está por culpa de un sondeo que falló. El caso real: las notificaciones se
 * consultan cada 15 segundos; si la API tose un momento, el usuario acabaría en
 * la pantalla de error sin haber tocado nada.
 *
 * Se usa el HttpContext de Angular en vez de mirar la URL porque el mismo
 * endpoint puede consultarse de las dos formas — la lista de notificaciones la
 * pide tanto el sondeo como la página "Mis seguimientos" — y quien sabe si el
 * usuario está esperando una respuesta es quien hace la llamada, no la ruta.
 */
export const PETICION_DE_FONDO = new HttpContextToken<boolean>(() => false);

/** Contexto listo para pasar a HttpClient en una llamada de fondo. */
export function contextoDeFondo(): HttpContext {
  return new HttpContext().set(PETICION_DE_FONDO, true);
}
