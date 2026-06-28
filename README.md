# SIV — Sistema de Información de Vuelos

Arquitectura de **monolito modular** en .NET 8.

---

## Responsable de esta rama

**Luis Miguel Mata Miranda**

### Módulos
| Módulo | Descripción |
|--------|-------------|
| Vuelos | Registro y consulta de vuelos |
| Gestión de estados | Máquina de estados del vuelo (Programado → Embarcando → EnVuelo → Aterrizado → Completado / Cancelado) |
| Cambios operativos | Retrasos, adelantos, cambios de puerta y cancelaciones |
| Catálogos | Administración de aerolíneas y aeropuertos |

### Aplicación
**Intranet** — App web para el personal del aeropuerto (agentes, operadores y administradores).

---

## Estructura del proyecto

| Proyecto | Rol |
|----------|-----|
| `SIV.API` | Expone los endpoints HTTP (REST) |
| `SIV.Modules` | Lógica de negocio por módulo (Domain + Application) |
| `SIV.Infrastructure` | Persistencia con EF Core + SQL Server |
| `SIV.Shared` | DTOs, eventos y excepciones compartidas |

---

## Requisitos previos

- .NET 8 SDK
- Docker Desktop

---

## Levantar el proyecto

**1. Iniciar la base de datos:**
```bash
docker compose up -d
```

**2. Crear el archivo de configuración local** (no está en el repo):
```
SIV.API/appsettings.Development.json
```
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,1433;Database=SivDb;User Id=sa;Password=Siv2026!Vuelos;TrustServerCertificate=True;"
  }
}
```

**3. Aplicar migraciones:**
```bash
dotnet ef database update --project SIV.Infrastructure --startup-project SIV.API
```

**4. Correr la API:**
```bash
dotnet run --project SIV.API
```

**5. Abrir Swagger:**
```
http://localhost:5102/swagger
```

---

## Endpoints principales

### Vuelos
| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/api/vuelos` | Listar todos los vuelos |
| GET | `/api/vuelos/{id}` | Obtener vuelo por ID |
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
