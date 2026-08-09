import { Component } from '@angular/core';
import { PantallaMarco } from '../../shared/pantalla-marco/pantalla-marco';

/**
 * Directorio de la terminal: plano esquemático, leyenda y servicios.
 *
 * Como la señalización, describe el edificio y no los vuelos: no consulta la
 * API. El plano es un SVG dibujado a mano en la plantilla, no una imagen — así
 * se ve nítido en cualquier tamaño de pantalla y pesa unos pocos kilobytes.
 */
@Component({
  selector: 'app-directorio',
  imports: [PantallaMarco],
  templateUrl: './directorio.html',
  styleUrl: './directorio.scss',
})
export class Directorio {
  /** Puertas dibujadas en el plano, con su posición dentro del esquema. */
  readonly puertas = [
    { nombre: 'A1', x: 90 },
    { nombre: 'A3', x: 190 },
    { nombre: 'A5', x: 290 },
    { nombre: 'A7', x: 390 },
    { nombre: 'A9', x: 490 },
    { nombre: 'A11', x: 590 },
  ];

  readonly leyenda = [
    { marca: 'puerta',    nombre: 'Puertas de embarque', nombreEn: 'Boarding gates', ubicacion: 'A1 – A12' },
    { marca: 'servicio',  nombre: 'Sanitarios',          nombreEn: 'Restrooms',      ubicacion: 'Zonas A y B' },
    { marca: 'comida',    nombre: 'Área de comidas',     nombreEn: 'Food court',     ubicacion: 'Plaza central' },
    { marca: 'lounge',    nombre: 'Salas VIP',           nombreEn: 'VIP lounges',    ubicacion: 'Nivel 3' },
  ];

  readonly servicios = [
    { nombre: 'Asistencia médica', nombreEn: 'Medical' },
    { nombre: 'Cambio de divisas', nombreEn: 'Currency exchange' },
    { nombre: 'WiFi gratuito',     nombreEn: 'Free WiFi' },
    { nombre: 'Información',       nombreEn: 'Help desk' },
  ];
}
