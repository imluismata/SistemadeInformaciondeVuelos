import { ResolveFn, Routes } from '@angular/router';
import { BuscarVuelos } from './pages/buscar-vuelos/buscar-vuelos';
import { DetalleVuelo } from './pages/detalle-vuelo/detalle-vuelo';
import { Inicio } from './pages/inicio/inicio';
import { Login } from './pages/login/login';
import { MisSeguimientos } from './pages/mis-seguimientos/mis-seguimientos';
import { Registro } from './pages/registro/registro';
import { Verificar } from './pages/verificar/verificar';
import { Recuperar } from './pages/recuperar/recuperar';
import { PaginaError } from './pages/error/pagina-error';
import { Tablero } from './pages/tablero/tablero';
import { Notificaciones } from './pages/notificaciones/notificaciones';
import { ETicket } from './pages/eticket/eticket';
import { Senalizacion } from './pages/senalizacion/senalizacion';
import { Directorio } from './pages/directorio/directorio';
import { Embarque } from './pages/embarque/embarque';
import { Pantallas } from './pages/pantallas/pantallas';

// El título del error depende del código de la URL, así que se resuelve al vuelo.
const tituloError: ResolveFn<string> = (ruta) => {
  const codigo = Number(ruta.paramMap.get('codigo'));
  return `Error ${[403, 404, 500, 503].includes(codigo) ? codigo : 404}`;
};

// El `title` de cada ruta es el nombre que se ve en el menú; TituloQuisqueya le
// agrega " · Quisqueya Flight Hub" para la pestaña del navegador.
export const routes: Routes = [
  { path: '', component: Inicio, title: 'Inicio' },
  { path: 'vuelos', component: BuscarVuelos, title: 'Vuelos' },
  { path: 'vuelos/:id', component: DetalleVuelo, title: 'Detalle del vuelo' },
  // Tablero para las pantallas de la terminal: se abre a pantalla completa, sin menú.
  // Panel desde el que se abren todas las pantallas. Esta sí es página de
  // portal (lleva menú); las que abre van a pantalla completa.
  { path: 'pantallas', component: Pantallas, title: 'Pantallas' },
  // Un mismo tablero sirve las tres pantallas; solo cambia el filtro.
  { path: 'tablero', component: Tablero, title: 'Tablero de vuelos', data: { modo: 'mixto' } },
  { path: 'tablero/salidas', component: Tablero, title: 'Salidas', data: { modo: 'salidas' } },
  { path: 'tablero/llegadas', component: Tablero, title: 'Llegadas', data: { modo: 'llegadas' } },
  // Cada puerta física abre su propia dirección: /tablero/embarque/A3
  { path: 'tablero/embarque/:puerta', component: Embarque, title: 'Puerta de embarque' },
  // Pantallas informativas del edificio: no consultan datos de vuelos.
  { path: 'tablero/eticket', component: ETicket, title: 'E-Ticket' },
  { path: 'tablero/senalizacion', component: Senalizacion, title: 'Señalización' },
  { path: 'tablero/directorio', component: Directorio, title: 'Directorio' },
  { path: 'login', component: Login, title: 'Iniciar sesión' },
  { path: 'registro', component: Registro, title: 'Registrarse' },
  { path: 'verificar', component: Verificar, title: 'Verificar correo' },
  { path: 'recuperar', component: Recuperar, title: 'Recuperar contraseña' },
  { path: 'mis-seguimientos', component: MisSeguimientos, title: 'Mis seguimientos' },
  // Historial de avisos; sirve igual al usuario con cuenta y al visitante anónimo.
  { path: 'notificaciones', component: Notificaciones, title: 'Notificaciones' },
  // Páginas de error con la marca; el interceptor navega aquí ante fallos de API.
  { path: 'error/:codigo', component: PaginaError, title: tituloError },
  // Cualquier ruta desconocida muestra el 404 (antes redirigía a home en silencio).
  { path: '**', component: PaginaError, title: 'Error 404' },
];
