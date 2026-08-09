import { Injectable, signal } from '@angular/core';

const DISPOSITIVO_KEY = 'siv_dispositivo_id';

/**
 * Identificador anónimo del navegador, para que un visitante pueda seguir
 * vuelos sin crear una cuenta.
 *
 * Es un Guid aleatorio que se genera la primera vez y se guarda en
 * localStorage. No lleva ningún dato personal: para el servidor es solo "el
 * seguidor número tal". Con él, el backend crea los mismos seguimientos y
 * notificaciones que para un usuario registrado — y como no corresponde a
 * ninguna cuenta, nunca se le envía correo, que es justo lo que se busca.
 *
 * Limitación asumida: vive en el navegador. Si la persona borra los datos del
 * sitio o entra desde otro dispositivo, empieza de cero. Es el precio de no
 * pedirle una cuenta.
 */
@Injectable({ providedIn: 'root' })
export class DispositivoService {
  private readonly _id = signal<string | null>(localStorage.getItem(DISPOSITIVO_KEY));

  /**
   * Id ya existente, o null si este navegador todavía no ha seguido nada.
   * Es una señal para que el sondeo de notificaciones arranque solo en cuanto
   * el visitante sigue su primer vuelo.
   */
  readonly id = this._id.asReadonly();

  /** Id de este navegador; lo crea la primera vez que hace falta de verdad. */
  obtenerId(): string {
    let id = this._id();

    if (!id) {
      id = crypto.randomUUID();
      localStorage.setItem(DISPOSITIVO_KEY, id);
      this._id.set(id);
    }

    return id;
  }
}
