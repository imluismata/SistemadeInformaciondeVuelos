import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { Notificacion } from '../models/notificacion.model';

@Injectable({ providedIn: 'root' })
export class NotificacionesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/notificaciones`;

  obtenerPorUsuario(usuarioId: string): Observable<Notificacion[]> {
    return this.http.get<Notificacion[]>(`${this.baseUrl}/${usuarioId}`);
  }

  marcarComoLeida(id: string): Observable<void> {
    return this.http.patch<void>(`${this.baseUrl}/${id}/leida`, {});
  }
}
