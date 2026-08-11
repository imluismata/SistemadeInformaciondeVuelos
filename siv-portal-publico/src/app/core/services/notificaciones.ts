import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notificacion } from '../models/notificacion.model';
import { contextoDeFondo } from '../peticion-de-fondo';

@Injectable({ providedIn: 'root' })
export class NotificacionesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/notificaciones`;

  /**
   * @param enSegundoPlano lo pone el sondeo automático, para que un fallo no
   * arrastre al usuario a la página de error (ver PETICION_DE_FONDO).
   */
  obtenerPorUsuario(usuarioId: string, enSegundoPlano = false): Observable<Notificacion[]> {
    return this.http.get<Notificacion[]>(`${this.baseUrl}/${usuarioId}`, {
      context: enSegundoPlano ? contextoDeFondo() : undefined,
    });
  }

  marcarComoLeida(id: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/leida`, {});
  }

  // ── Visitante sin cuenta: se identifica con el id del navegador. El servidor
  // comprueba que la notificación sea suya antes de marcarla.

  obtenerPorDispositivo(dispositivoId: string, enSegundoPlano = false): Observable<Notificacion[]> {
    return this.http.get<Notificacion[]>(`${this.baseUrl}/anonimo/${dispositivoId}`, {
      context: enSegundoPlano ? contextoDeFondo() : undefined,
    });
  }

  marcarComoLeidaAnonima(id: string, dispositivoId: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/anonimo/${id}/leida`, { dispositivoId });
  }
}
