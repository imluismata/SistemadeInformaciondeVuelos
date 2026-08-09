import { CommonModule } from '@angular/common';
import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Observable } from 'rxjs';
import { VuelosService } from '../../core/services/vuelos';
import { SeguimientoService } from '../../core/services/seguimiento';
import { AuthService } from '../../core/services/auth';
import { DispositivoService } from '../../core/services/dispositivo';
import { VueloPublico } from '../../core/models/vuelo.model';
import { etiquetaEstado } from '../../core/estados-vuelo';
import { EstadoVuelo } from '../../shared/estado-vuelo/estado-vuelo';
import { LogoAerolinea } from '../../shared/logo-aerolinea/logo-aerolinea';
import { mensajeDeError } from '../../core/errores';

@Component({
  selector: 'app-detalle-vuelo',
  imports: [CommonModule, RouterLink, EstadoVuelo, LogoAerolinea],
  templateUrl: './detalle-vuelo.html',
  styleUrl: './detalle-vuelo.scss',
})
export class DetalleVuelo implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly vuelosService = inject(VuelosService);
  private readonly seguimientoService = inject(SeguimientoService);
  private readonly dispositivo = inject(DispositivoService);
  readonly auth = inject(AuthService);

  readonly vuelo = signal<VueloPublico | null>(null);
  readonly cargando = signal(true);
  readonly mensaje = signal<string | null>(null);
  readonly mensajeEsError = signal(false);

  /** Si este visitante (cuenta o navegador) ya sigue el vuelo. */
  readonly siguiendo = signal(false);

  /**
   * Texto del cambio cuando se llega aquí haciendo clic en una notificación.
   * Viaja en el state de la navegación, así no hace falta pedirlo a la API.
   */
  readonly mensajeCambio = signal<string | null>(null);

  /** Rótulo del estado tal como se muestra en pantalla ("A tiempo", "Retrasado"...). */
  readonly etiqueta = computed(() => etiquetaEstado(this.vuelo()?.estado ?? ''));

  ngOnInit(): void {
    const estado = history.state as { mensajeCambio?: string } | null;
    this.mensajeCambio.set(estado?.mensajeCambio ?? null);

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.vuelosService.obtenerDetalle(id).subscribe({
      next: (vuelo) => { this.vuelo.set(vuelo); this.cargando.set(false); },
      error: () => this.cargando.set(false),
    });

    this.comprobarSiLoSigue(id);
  }

  /** Alterna entre seguir y dejar de seguir, según el estado actual. */
  alternarSeguimiento(): void {
    const vuelo = this.vuelo();
    if (!vuelo) return;

    if (this.siguiendo()) this.dejarDeSeguir(vuelo.id);
    else this.seguir(vuelo.id);
  }

  private seguir(vueloId: string): void {
    // Un usuario con sesión se identifica con su cuenta; un visitante sin
    // cuenta, con el id de este navegador. Para el backend es el mismo servicio.
    const usuario = this.auth.usuarioActual();
    const peticion = usuario
      ? this.seguimientoService.registrar({ usuarioId: usuario.id, vueloId })
      : this.seguimientoService.registrarAnonimo(this.dispositivo.obtenerId(), vueloId);

    peticion.subscribe({
      next: () => {
        this.siguiendo.set(true);
        this.mensajeEsError.set(false);
        this.mensaje.set(
          usuario
            ? 'Ahora sigues este vuelo. Recibirás notificaciones aquí y por correo.'
            : 'Ahora sigues este vuelo. Te avisaremos aquí mismo cuando cambie.'
        );
      },
      error: (e) => {
        // 409 = el backend indica que ya existe un seguimiento activo para este vuelo.
        if (e?.status === 409) {
          this.siguiendo.set(true);
          this.mensajeEsError.set(false);
          this.mensaje.set('Ya estás siguiendo este vuelo.');
        } else {
          this.mensajeEsError.set(true);
          this.mensaje.set(mensajeDeError(e, 'No se pudo registrar el seguimiento.'));
        }
      },
    });
  }

  private dejarDeSeguir(vueloId: string): void {
    const usuario = this.auth.usuarioActual();
    const peticion = usuario
      ? this.seguimientoService.cancelar({ usuarioId: usuario.id, vueloId })
      : this.seguimientoService.cancelarAnonimo(this.dispositivo.obtenerId(), vueloId);

    peticion.subscribe({
      next: () => {
        this.siguiendo.set(false);
        this.mensajeEsError.set(false);
        this.mensaje.set('Dejaste de seguir este vuelo.');
      },
      error: (e) => {
        this.mensajeEsError.set(true);
        this.mensaje.set(mensajeDeError(e, 'No se pudo cancelar el seguimiento.'));
      },
    });
  }

  /**
   * Pregunta si el vuelo ya está entre los seguimientos activos, para que el
   * botón muestre "Seguir" o "Dejar de seguir" desde el principio. El visitante
   * sin cuenta solo se consulta si ya tiene id, para no crear uno a quien
   * simplemente está mirando vuelos.
   */
  private comprobarSiLoSigue(vueloId: string): void {
    const usuario = this.auth.usuarioActual();
    const dispositivoId = this.dispositivo.id();

    let consulta: Observable<{ vueloId: string }[]> | null = null;
    if (usuario) consulta = this.seguimientoService.obtenerPorUsuario(usuario.id);
    else if (dispositivoId) consulta = this.seguimientoService.obtenerPorDispositivo(dispositivoId);

    consulta?.subscribe({
      next: (seguimientos) => this.siguiendo.set(seguimientos.some((s) => s.vueloId === vueloId)),
      error: () => {},
    });
  }
}
