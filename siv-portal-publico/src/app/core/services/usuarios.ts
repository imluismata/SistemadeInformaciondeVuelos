import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, RegistroUsuarioRequest, Usuario } from '../models/usuario.model';

@Injectable({ providedIn: 'root' })
export class UsuariosService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.apiUrl}/usuarios`;

  registrar(request: RegistroUsuarioRequest): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/registro`, request);
  }

  login(request: LoginRequest): Observable<Usuario> {
    return this.http.post<Usuario>(`${this.baseUrl}/login`, request);
  }

  obtenerPorId(id: string): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.baseUrl}/${id}`);
  }
}
