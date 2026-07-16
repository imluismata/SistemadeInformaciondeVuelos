import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { VuelosService } from '../../core/services/vuelos';
import { SeguimientoService } from '../../core/services/seguimiento';
import { AuthService } from '../../core/services/auth';
import { VueloPublico } from '../../core/models/vuelo.model';

@Component({
  selector: 'app-detalle-vuelo',
  imports: [CommonModule],
  templateUrl: './detalle-vuelo.html',
  styleUrl: './detalle-vuelo.scss',
})
export class DetalleVuelo implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly vuelosService = inject(VuelosService);
  private readonly seguimientoService = inject(SeguimientoService);
  readonly auth = inject(AuthService);

  readonly vuelo = signal<VueloPublico | null>(null);
  readonly cargando = signal(true);
  readonly mensaje = signal<string | null>(null);

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.vuelosService.obtenerDetalle(id).subscribe({
      next: (vuelo) => {
        this.vuelo.set(vuelo);
        this.cargando.set(false);
      },
      error: () => this.cargando.set(false),
    });
  }

  seguirVuelo(): void {
    const usuario = this.auth.usuarioActual();
    const vuelo = this.vuelo();
    if (!usuario || !vuelo) return;

    this.seguimientoService
      .registrar({ usuarioId: usuario.id, vueloId: vuelo.id })
      .subscribe({
        next: () => this.mensaje.set('Ahora sigues este vuelo.'),
        error: () => this.mensaje.set('No se pudo registrar el seguimiento.'),
      });
  }
}
