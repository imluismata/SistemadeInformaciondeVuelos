import { Routes } from '@angular/router';
import { BuscarVuelos } from './pages/buscar-vuelos/buscar-vuelos';
import { DetalleVuelo } from './pages/detalle-vuelo/detalle-vuelo';
import { Inicio } from './pages/inicio/inicio';
import { Login } from './pages/login/login';
import { MisSeguimientos } from './pages/mis-seguimientos/mis-seguimientos';
import { Registro } from './pages/registro/registro';
import { Verificar } from './pages/verificar/verificar';
import { Recuperar } from './pages/recuperar/recuperar';

export const routes: Routes = [
  { path: '', component: Inicio },
  { path: 'vuelos', component: BuscarVuelos },
  { path: 'vuelos/:id', component: DetalleVuelo },
  { path: 'login', component: Login },
  { path: 'registro', component: Registro },
  { path: 'verificar', component: Verificar },
  { path: 'recuperar', component: Recuperar },
  { path: 'mis-seguimientos', component: MisSeguimientos },
  { path: '**', redirectTo: '' },
];
