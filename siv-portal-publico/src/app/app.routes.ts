import { Routes } from '@angular/router';
import { BuscarVuelos } from './pages/buscar-vuelos/buscar-vuelos';
import { DetalleVuelo } from './pages/detalle-vuelo/detalle-vuelo';
import { Inicio } from './pages/inicio/inicio';
import { Login } from './pages/login/login';
import { MisSeguimientos } from './pages/mis-seguimientos/mis-seguimientos';
import { Registro } from './pages/registro/registro';

export const routes: Routes = [
  { path: '', component: Inicio },
  { path: 'vuelos', component: BuscarVuelos },
  { path: 'vuelos/:id', component: DetalleVuelo },
  { path: 'login', component: Login },
  { path: 'registro', component: Registro },
  { path: 'mis-seguimientos', component: MisSeguimientos },
  { path: '**', redirectTo: '' },
];
