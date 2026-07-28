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

  verificarCodigo(email: string, codigo: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/verificar`, { email, codigo });
  }

  reenviarCodigo(email: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/reenviar-codigo`, { email });
  }

  recuperarPassword(email: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/recuperar`, { email });
  }

  validarCodigo(email: string, codigo: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/validar-codigo`, { email, codigo });
  }

  restablecerPassword(email: string, codigo: string, nuevaPassword: string): Observable<void> {
    return this.http.post<void>(`${this.baseUrl}/restablecer`, { email, codigo, nuevaPassword });
  }

  login(request: LoginRequest): Observable<Usuario> {
    return this.http.post<Usuario>(`${this.baseUrl}/login`, request);
  }

  obtenerPorId(id: string): Observable<Usuario> {
    return this.http.get<Usuario>(`${this.baseUrl}/${id}`);
  }
}
