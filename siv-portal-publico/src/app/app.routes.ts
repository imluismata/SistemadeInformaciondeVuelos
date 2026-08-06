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
  { path: 'login', component: Login, title: 'Iniciar sesión' },
  { path: 'registro', component: Registro, title: 'Registrarse' },
  { path: 'verificar', component: Verificar, title: 'Verificar correo' },
  { path: 'recuperar', component: Recuperar, title: 'Recuperar contraseña' },
  { path: 'mis-seguimientos', component: MisSeguimientos, title: 'Mis seguimientos' },
  // Páginas de error con la marca; el interceptor navega aquí ante fallos de API.
  { path: 'error/:codigo', component: PaginaError, title: tituloError },
  // Cualquier ruta desconocida muestra el 404 (antes redirigía a home en silencio).
  { path: '**', component: PaginaError, title: 'Error 404' },
];
