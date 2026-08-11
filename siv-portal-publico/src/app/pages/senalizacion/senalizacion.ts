import { Component } from '@angular/core';
import { PantallaMarco } from '../../shared/pantalla-marco/pantalla-marco';

type Direccion = 'arriba' | 'izquierda' | 'derecha';

interface Indicacion {
  direccion: Direccion;
  destino: string;
  destinoEn: string;
  detalle: string;
  servicios: string[];
  principal: boolean;
}

/**
 * Señalización direccional de pasillo: la pantalla que le dice al pasajero
 * hacia dónde caminar.
 *
 * Es contenido fijo, propio del edificio y no de los vuelos, así que vive en el
 * componente. El día que el aeropuerto cambie la distribución, se edita esta
 * lista; no hay nada que consultar a la API.
 */
@Component({
  selector: 'app-senalizacion',
  imports: [PantallaMarco],
  templateUrl: './senalizacion.html',
  styleUrl: './senalizacion.scss',
})
export class Senalizacion {
  readonly indicaciones: Indicacion[] = [
    {
      direccion: 'arriba',
      destino: 'Puertas A1 – A20',
      destinoEn: 'Gates A1 – A20',
      detalle: 'Salidas internacionales · International departures',
      servicios: ['Sanitarios · Restrooms', 'Comidas · Food court', 'Sala VIP · Lounge'],
      principal: true,
    },
    {
      direccion: 'izquierda',
      destino: 'Reclamo de equipaje',
      destinoEn: 'Baggage claim',
      detalle: 'Transporte terrestre · Ground transportation',
      servicios: ['Taxis', 'Autobuses · Buses', 'Alquiler de autos · Car rental'],
      principal: false,
    },
    {
      direccion: 'derecha',
      destino: 'Puertas B1 – B15',
      destinoEn: 'Gates B1 – B15',
      detalle: 'Vuelos regionales · Regional flights',
      servicios: ['Cafetería · Coffee', 'Sanitarios · Restrooms'],
      principal: false,
    },
  ];
}
