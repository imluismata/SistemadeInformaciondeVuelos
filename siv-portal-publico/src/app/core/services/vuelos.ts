import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { FiltroConsulta, VueloPublico } from '../models/vuelo.model';

@Injectable({ providedIn: 'root' })
export class VuelosService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/consulta-publica`;

  obtenerActivos(): Observable<VueloPublico[]> {
    return this.http.get<VueloPublico[]>(this.baseUrl);
  }

  buscarPorNumero(numero: string): Observable<VueloPublico> {
    return this.http.get<VueloPublico>(`${this.baseUrl}/buscar`, {
      params: new HttpParams().set('numero', numero),
    });
  }

  buscarConFiltro(filtro: FiltroConsulta): Observable<VueloPublico[]> {
    let params = new HttpParams();
    if (filtro.origen) params = params.set('origen', filtro.origen);
    if (filtro.destino) params = params.set('destino', filtro.destino);
    if (filtro.fecha) params = params.set('fecha', filtro.fecha);

    return this.http.get<VueloPublico[]>(`${this.baseUrl}/filtro`, { params });
  }

  obtenerDetalle(id: string): Observable<VueloPublico> {
    return this.http.get<VueloPublico>(`${this.baseUrl}/${id}`);
  }
}
