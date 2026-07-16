import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  CancelarSeguimientoRequest,
  RegistrarSeguimientoRequest,
  Seguimiento as SeguimientoModel,
} from '../models/seguimiento.model';

@Injectable({ providedIn: 'root' })
export class SeguimientoService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/seguimiento`;

  registrar(request: RegistrarSeguimientoRequest): Observable<void> {
    return this.http.post<void>(this.baseUrl, request);
  }

  cancelar(request: CancelarSeguimientoRequest): Observable<void> {
    return this.http.delete<void>(this.baseUrl, { body: request });
  }

  obtenerPorUsuario(usuarioId: string): Observable<SeguimientoModel[]> {
    return this.http.get<SeguimientoModel[]>(`${this.baseUrl}/usuario/${usuarioId}`);
  }
}
