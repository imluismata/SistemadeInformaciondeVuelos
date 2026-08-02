import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';

interface InfoError {
  etiqueta: string;
  titulo: string;
  mensaje: string;
  lectura: string;
}

// Textos por código de error. Los SVG viven en la plantilla (uno por código).
const ERRORES: Record<number, InfoError> = {
  403: {
    etiqueta: 'Error 403 · Acceso denegado',
    titulo: 'Zona restringida',
    mensaje: 'No tienes autorización para entrar a esta área. Inicia sesión con una cuenta con permiso o vuelve al inicio.',
    lectura: 'NIVEL: CRITICAL_AUTH · ESTADO: DENEGADO',
  },
  404: {
    etiqueta: 'Error 404 · Página no encontrada',
    titulo: 'Fuera de radar',
    mensaje: 'No encontramos lo que buscabas. Este destino no aparece en nuestro radar.',
    lectura: 'ALTITUD: UNK-000 · RUMBO: PERDIDO',
  },
  500: {
    etiqueta: 'Error 500 · Fallo interno',
    titulo: 'Turbulencia técnica',
    mensaje: 'Algo salió mal de nuestro lado. Nuestro equipo ya está en ello — intenta de nuevo en un momento.',
    lectura: 'SISTEMA: EN TIERRA · REINTENTO SUGERIDO',
  },
  503: {
    etiqueta: 'Error 503 · Servicio no disponible',
    titulo: 'No podemos despegar',
    mensaje: 'No pudimos conectar con el servidor. El sistema está en tierra por un momento — intenta de nuevo en unos segundos.',
    lectura: 'TORRE DE CONTROL: SIN RESPUESTA',
  },
};

@Component({
  selector: 'app-pagina-error',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './pagina-error.html',
  styleUrl: './pagina-error.scss',
})
export class PaginaError {
  private readonly route = inject(ActivatedRoute);

  codigo = 404;
  info: InfoError = ERRORES[404];

  constructor() {
    // El catch-all (**) no trae parámetro → 404. Las rutas /error/:codigo sí.
    this.route.paramMap.subscribe((p) => {
      const c = Number(p.get('codigo'));
      this.codigo = ERRORES[c] ? c : 404;
      this.info = ERRORES[this.codigo];
    });
  }

  reintentar(): void {
    window.location.reload();
  }
}
