import { Component, signal } from '@angular/core';
import { PantallaMarco } from '../../shared/pantalla-marco/pantalla-marco';

/**
 * Pantalla informativa del E-Ticket de Migración.
 *
 * No consulta nada: es un cartel digital con instrucciones fijas. Por eso no
 * tiene servicios inyectados ni estado — si mañana cambia el texto, se cambia
 * aquí y ya.
 */
@Component({
  selector: 'app-eticket',
  imports: [PantallaMarco],
  templateUrl: './eticket.html',
  styleUrl: './eticket.scss',
})
export class ETicket {
  readonly url = 'eticket.migracion.gob.do';

  /**
   * Si el PNG del código no está en public/, la pantalla no muestra el icono de
   * imagen rota: enseña la dirección escrita, que es lo que la persona necesita
   * para llegar al formulario de todos modos.
   */
  readonly qrDisponible = signal(true);

  readonly pasos = [
    {
      numero: 1,
      titulo: 'Escanea el código QR',
      tituloEn: 'Scan the QR code',
      detalle: 'Con la cámara de tu teléfono, no necesitas ninguna aplicación.',
    },
    {
      numero: 2,
      titulo: 'Completa el formulario',
      tituloEn: 'Fill out the form',
      detalle: 'Tus datos personales y los de tu vuelo.',
    },
    {
      numero: 3,
      titulo: 'Guarda tu código',
      tituloEn: 'Save your code',
      detalle: 'Preséntalo en el punto de control migratorio.',
    },
  ];
}
