# SIV — Sistema de Información de Vuelos

Arquitectura de **monolito modular** en .NET 8.

---

## Integrantes

### Luis Miguel Mata Miranda
| Módulo | Descripción |
|--------|-------------|
| Vuelos | Registro y consulta de vuelos |
| Gestión de estados | Máquina de estados del vuelo (Programado → Embarcando → EnVuelo → Aterrizado → Completado / Cancelado) |
| Cambios operativos | Retrasos, adelantos, cambios de puerta y cancelaciones |
| Catálogos | Administración de aerolíneas y aeropuertos |
| Auditoría | Registro centralizado e inmutable de todas las acciones del sistema |

**Aplicación:** Intranet — app web para el personal del aeropuerto (agentes, operadores y administradores).

---

### Paola Piantini
| Módulo | Descripción |
|--------|-------------|
| Usuarios | Registro, login y gestión de roles |
| Seguimiento | Suscripción de usuarios a vuelos para recibir notificaciones |
| Notificaciones | Generación y consulta de notificaciones ante cambios en vuelos |
| Consulta pública | Búsqueda y detalle de vuelos sin necesidad de autenticarse |

**Aplicación:** App web para pasajeros — consulta de vuelos y gestión de su cuenta.

---

## Estructura del proyecto

| Proyecto | Rol |
|----------|-----|
| `SIV.API` | Expone los endpoints HTTP (REST) |
| `SIV.Modules` | Lógica de negocio por módulo (Domain + Application) |
| `SIV.Infrastructure` | Persistencia con EF Core + SQL Server |
| `SIV.Shared` | DTOs, eventos y excepciones compartidas |

---

## Endpoints principales

### Vuelos
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/vuelos` | Listar todos los vuelos |
| GET | `/api/vuelos/{id}` | Obtener vuelo por ID |
| GET | `/api/vuelos/consultar` | Consultar con filtros (aerolínea, ruta, fecha, estado) |
| POST | `/api/vuelos` | Registrar vuelo |
| PUT | `/api/vuelos/{id}` | Actualizar datos del vuelo |
| PUT | `/api/vuelos/{id}/estado` | Cambiar estado |
| POST | `/api/vuelos/{id}/cambios-operativos` | Registrar retraso, adelanto, cambio de puerta o cancelación |

### Catálogos
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/catalogo/aerolineas` | Listar aerolíneas |
| POST | `/api/catalogo/aerolineas` | Registrar aerolínea |
| PUT | `/api/catalogo/aerolineas/{id}` | Actualizar aerolínea |
| DELETE | `/api/catalogo/aerolineas/{id}` | Desactivar aerolínea |
| GET | `/api/catalogo/aeropuertos` | Listar aeropuertos |
| POST | `/api/catalogo/aeropuertos` | Registrar aeropuerto |
| PUT | `/api/catalogo/aeropuertos/{id}` | Actualizar aeropuerto |
| DELETE | `/api/catalogo/aeropuertos/{id}` | Desactivar aeropuerto |

### Auditoría
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/auditoria` | Consultar registros de auditoría (filtros: módulo, acción, fecha desde/hasta) |

### Usuarios
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/usuarios/registro` | Registrar nuevo usuario |
| POST | `/api/usuarios/login` | Iniciar sesión |
| GET | `/api/usuarios/{id}` | Obtener perfil por Id |
| PATCH | `/api/usuarios/{id}/rol` | Cambiar rol de un usuario |

### Seguimiento
| Método | Ruta | Descripción |
|--------|------|-------------|
| POST | `/api/seguimiento` | Suscribirse a un vuelo |
| DELETE | `/api/seguimiento` | Cancelar suscripción |
| GET | `/api/seguimiento/vuelo/{vueloId}` | Ver usuarios suscritos a un vuelo |

### Notificaciones
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/notificaciones/{usuarioId}` | Ver notificaciones de un usuario |
| PATCH | `/api/notificaciones/{id}/leida` | Marcar notificación como leída |

### Consulta pública
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/vuelos` | Listar vuelos activos del día |
| GET | `/api/vuelos/buscar?numero=XX123` | Buscar vuelo por número |
| GET | `/api/vuelos/{id}` | Ver detalle de un vuelo |
