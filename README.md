# FinanzasPersonales.Api

API REST desarrollada con **.NET 8** para gestión integral de finanzas personales. Provee autenticación con Google OAuth + JWT, gestión multi-cuenta, presupuestos multi-periodo, deudas, gastos compartidos, transacciones recurrentes, reportes avanzados, notificaciones en tiempo real y jobs automatizados.

## Tecnologias

| Tecnologia | Uso |
|---|---|
| .NET 8 | Framework principal |
| Entity Framework Core 9 | ORM con PostgreSQL |
| ASP.NET Core Identity | Autenticacion y usuarios |
| JWT Bearer | Tokens de autenticacion (2h expiracion) |
| Google OAuth | Login con Google |
| Hangfire | Jobs recurrentes en background |
| SignalR | Notificaciones en tiempo real |
| PostgreSQL | Base de datos relacional |
| QuestPDF | Generacion de reportes PDF |
| EPPlus | Exportacion a Excel |
| iTextSharp | Procesamiento de PDFs |
| MailKit | Envio de emails |
| CsvHelper | Importacion de CSV |
| Swagger/OpenAPI | Documentacion interactiva |

## Estructura del Proyecto

```
FinanzasPersonales.Api/
├── Controllers/           # 27 controladores REST
├── Services/              # 20+ servicios con interfaces
├── Models/                # 20+ entidades de dominio
├── Dtos/                  # 70+ Data Transfer Objects
├── Data/
│   ├── FinanzasDbContext.cs
│   └── FinanzasDbContextFactory.cs
├── Jobs/
│   ├── NotificacionesJob.cs        # Alertas diarias (9:00 AM)
│   ├── RecurrentesJob.cs           # Genera transacciones (cada hora)
│   └── ReportesProgramadosJob.cs   # Envia reportes (7:00 AM)
├── Hubs/
│   └── NotificacionesHub.cs        # WebSocket real-time
├── Migrations/            # 24 migraciones
├── Program.cs             # Configuracion completa
├── appsettings.json
└── appsettings.Production.json
```

## Endpoints de la API

### Autenticacion (`/api/Auth`)
| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/Auth/google` | Login con Google OAuth |
| GET | `/api/Auth/profile` | Obtener perfil del usuario |
| PUT | `/api/Auth/profile` | Actualizar nombre de usuario |

### Gastos (`/api/Gastos`)
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/Gastos` | Listar gastos (filtros, paginacion, tags) |
| GET | `/api/Gastos/{id}` | Obtener gasto por ID |
| POST | `/api/Gastos` | Crear gasto |
| PUT | `/api/Gastos/{id}` | Actualizar gasto |
| DELETE | `/api/Gastos/{id}` | Eliminar gasto |
| GET | `/api/Gastos/{id}/con-detalles` | Gasto con sub-compras |
| GET | `/api/Gastos/{id}/detalles` | Listar sub-compras |
| POST | `/api/Gastos/{id}/detalles` | Agregar sub-compra |
| PUT | `/api/Gastos/{id}/detalles/{detalleId}` | Editar sub-compra |
| DELETE | `/api/Gastos/{id}/detalles/{detalleId}` | Eliminar sub-compra |

### Ingresos (`/api/Ingresos`)
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/Ingresos` | Listar ingresos (filtros, paginacion, tags) |
| POST | `/api/Ingresos` | Crear ingreso |
| PUT | `/api/Ingresos/{id}` | Actualizar ingreso |
| DELETE | `/api/Ingresos/{id}` | Eliminar ingreso |

### Cuentas (`/api/Cuentas`)
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/Cuentas` | Listar cuentas |
| POST | `/api/Cuentas` | Crear cuenta (Efectivo, Bancaria, Credito, Ahorros, Inversion) |
| PUT | `/api/Cuentas/{id}` | Actualizar cuenta |
| DELETE | `/api/Cuentas/{id}` | Eliminar cuenta (soft delete) |
| GET | `/api/Cuentas/balance-total` | Balance total de todas las cuentas |

### Transferencias (`/api/Transferencias`)
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/Transferencias` | Listar transferencias |
| POST | `/api/Transferencias` | Crear transferencia entre cuentas |

### Presupuestos (`/api/Presupuestos`)
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/Presupuestos` | Listar presupuestos con progreso |
| POST | `/api/Presupuestos` | Crear presupuesto |
| PUT | `/api/Presupuestos/{id}` | Actualizar presupuesto |
| DELETE | `/api/Presupuestos/{id}` | Eliminar presupuesto |
| GET | `/api/Presupuestos/alertas` | Presupuestos >80% gastado |
| GET | `/api/Presupuestos/dashboard?periodo=` | Dashboard comparativo por periodo |

Periodos soportados: `Semanal`, `Quincenal`, `Mensual`, `Trimestral`, `Semestral`, `Anual`

### Metas (`/api/Metas`)
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/Metas` | Listar metas |
| POST | `/api/Metas` | Crear meta de ahorro |
| PUT | `/api/Metas/{id}` | Actualizar meta |
| DELETE | `/api/Metas/{id}` | Eliminar meta |
| POST | `/api/Metas/{id}/abonar` | Abonar a meta |
| GET | `/api/Metas/{id}/progreso` | Progreso detallado |
| GET | `/api/Metas/proyecciones` | Proyecciones de completado |

### Deudas (`/api/Deudas`)
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/Deudas` | Listar deudas |
| GET | `/api/Deudas/{id}` | Detalle de deuda |
| POST | `/api/Deudas` | Registrar deuda |
| PUT | `/api/Deudas/{id}` | Actualizar deuda |
| DELETE | `/api/Deudas/{id}` | Eliminar deuda |
| POST | `/api/Deudas/{id}/pagos` | Registrar pago (calcula interes/capital) |
| GET | `/api/Deudas/{id}/pagos` | Historial de pagos |
| GET | `/api/Deudas/{id}/proyeccion?pagoMensual=` | Proyeccion de liquidacion |

Tipos: `TarjetaCredito`, `PrestamoPersonal`, `Hipoteca`, `PrestamoAuto`, `Otro`

### Gastos Compartidos (`/api/gastos-compartidos`)
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/gastos-compartidos` | Listar gastos compartidos |
| GET | `/api/gastos-compartidos/resumen` | Resumen de quien debe cuanto |
| GET | `/api/gastos-compartidos/{id}` | Detalle con participantes |
| POST | `/api/gastos-compartidos` | Crear gasto compartido |
| DELETE | `/api/gastos-compartidos/{id}` | Eliminar |
| PUT | `/api/gastos-compartidos/{gastoId}/participantes/{participanteId}/liquidar` | Registrar pago de participante |

Metodos de division: `Equitativo`, `Porcentaje`, `MontoFijo`

### Recurrentes
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/GastosRecurrentes` | Listar gastos recurrentes |
| POST | `/api/GastosRecurrentes` | Crear |
| PUT | `/api/GastosRecurrentes/{id}` | Actualizar |
| DELETE | `/api/GastosRecurrentes/{id}` | Eliminar |
| POST | `/api/GastosRecurrentes/{id}/generar` | Generar gasto ahora |
| POST | `/api/GastosRecurrentes/generar-pendientes` | Generar todos los pendientes |
| GET | `/api/IngresosRecurrentes` | Listar ingresos recurrentes |
| POST | `/api/IngresosRecurrentes` | Crear |
| POST | `/api/IngresosRecurrentes/{id}/generar` | Generar ingreso ahora |

Frecuencias: `Semanal`, `Quincenal`, `Mensual`, `Anual`

### Dashboard y Reportes
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/Dashboard/metrics` | Metricas completas del dashboard |
| GET | `/api/Dashboard/flujo-caja` | Analisis de flujo de caja |
| GET | `/api/CuentaDashboard` | Dashboard por cuenta |
| GET | `/api/Reportes/tendencias?meses=` | Tendencias mensuales |
| GET | `/api/Reportes/comparativa` | Comparativa mes a mes |
| GET | `/api/Reportes/top-categorias` | Top categorias de gasto |
| GET | `/api/Reportes/gastos-tipo` | Gastos fijos vs variables |
| GET | `/api/Reportes/proyeccion` | Proyeccion del mes actual |
| GET | `/api/Reportes/calendario` | Vista calendario de transacciones |
| GET | `/api/Reportes/comparar-periodos` | Comparacion de periodos custom |

### Exportacion (`/api/Export`)
| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/Export/excel` | Exportar a Excel |
| POST | `/api/Export/pdf` | Exportar a PDF |
| POST | `/api/Export/backup` | Backup completo en JSON |

### Importacion (`/api/Importacion`)
| Metodo | Ruta | Descripcion |
|---|---|---|
| POST | `/api/Importacion/preview` | Preview de estructura CSV |
| POST | `/api/Importacion/validate` | Validar con mapeo (detecta duplicados) |
| POST | `/api/Importacion/ejecutar` | Ejecutar importacion |

### Herramientas de Automatizacion
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET | `/api/PlantillasGasto` | Listar plantillas de gasto |
| POST | `/api/PlantillasGasto` | Crear plantilla |
| POST | `/api/PlantillasGasto/{id}/usar` | Crear gasto desde plantilla |
| GET | `/api/ReglasCategoria` | Listar reglas de auto-categorizacion |
| POST | `/api/ReglasCategoria` | Crear regla |
| GET | `/api/ReglasCategoria/sugerir?descripcion=&tipo=` | Sugerir categoria |

### Otros
| Metodo | Ruta | Descripcion |
|---|---|---|
| GET/PUT | `/api/Configuracion` | Configuracion del usuario (moneda, idioma, tema) |
| GET | `/api/Notificaciones` | Listar notificaciones |
| GET | `/api/Notificaciones/no-leidas` | Conteo de no leidas |
| GET/PUT | `/api/Notificaciones/configuracion` | Config de alertas |
| CRUD | `/api/Categorias` | Categorias (jerarquicas con subcategorias) |
| CRUD | `/api/Tags` | Tags para transacciones |
| CRUD | `/api/Adjuntos` | Adjuntos PDF (max 5MB) |
| CRUD | `/api/TipoCambio` | Tipos de cambio multi-moneda |
| CRUD | `/api/ReportesProgramados` | Reportes automaticos por email |

## Jobs en Background (Hangfire)

| Job | Frecuencia | Descripcion |
|---|---|---|
| `verificar-alertas-diarias` | Diario 9:00 AM | Verifica presupuestos >80%, gastos inusuales, saldo bajo, pagos proximos |
| `generar-transacciones-recurrentes` | Cada hora | Genera gastos/ingresos recurrentes pendientes |
| `enviar-reportes-programados` | Diario 7:00 AM | Envia reportes semanales/mensuales por email |

## Notificaciones en Tiempo Real (SignalR)

Hub: `/hubs/notificaciones`
- Autenticacion por JWT (token en query string)
- Grupos por userId para entrega dirigida
- Evento: `NuevaNotificacion`

## Seguridad

- **JWT Bearer** con expiracion de 2 horas
- **Google OAuth** para login sin password
- **Rate Limiting**: 100 req/min global, 10 req/min en auth
- **Identity**: Password 8+ chars, mayusculas, minusculas, digitos, lockout tras 5 intentos
- **Security Headers**: CSP, X-Frame-Options, X-Content-Type-Options, X-XSS-Protection, HSTS
- **CORS**: Origenes restringidos, credenciales permitidas
- **Multi-tenancy**: Todos los datos filtrados por UserId del JWT
- **Validacion**: Data Annotations en todos los DTOs
- **Cascade/Restrict**: DeleteBehavior configurado por relacion

## Configuracion

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=finanzas_db;Username=postgres;Password=tu_password"
  },
  "Jwt": {
    "Key": "clave_secreta_minimo_32_caracteres",
    "Issuer": "FinanzasPersonalesApi",
    "Audience": "FinanzasPersonalesClients"
  },
  "Google": {
    "ClientId": "tu-google-client-id"
  },
  "CorsSettings": {
    "AllowedOrigins": ["http://localhost:5173", "https://tu-frontend.vercel.app"]
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "tu-email@gmail.com",
    "SenderName": "Finanzas Personales",
    "Username": "tu-email@gmail.com",
    "Password": "tu-app-password",
    "EnableSsl": true
  },
  "HangfireSettings": {
    "DashboardEnabled": true,
    "DashboardPath": "/hangfire"
  }
}
```

## Instalacion

### Prerrequisitos
- .NET 8 SDK
- PostgreSQL 12+

### Setup

```bash
# Clonar e ir al directorio
cd FinanzasPersonales.Api

# Restaurar dependencias
dotnet restore

# Configurar appsettings.json con tu connection string y JWT key

# Las migraciones se ejecutan automaticamente al iniciar
dotnet run
```

La API estara disponible en:
- `http://localhost:5030` (HTTP)
- `https://localhost:7173` (HTTPS)
- `http://localhost:5030/swagger` (Documentacion interactiva)
- `http://localhost:5030/hangfire` (Dashboard de jobs)

### Migraciones manuales

```bash
# Crear nueva migracion
dotnet ef migrations add NombreMigracion

# Aplicar migraciones
dotnet ef database update

# Revertir a migracion especifica
dotnet ef database update NombreMigracionAnterior
```

## Base de Datos

24 migraciones cubriendo: usuarios, gastos, ingresos, categorias (jerarquicas), presupuestos, metas, cuentas, transferencias, tags, notificaciones, configuracion, adjuntos, gastos recurrentes, ingresos recurrentes, notas, subcategorias, reglas de auto-categorizacion, importacion CSV, plantillas, deudas y pagos, gastos compartidos, alertas, multi-moneda, reportes programados, detalle de gastos.

## Tests

```bash
cd FinanzasPersonales.Tests
dotnet test
```

---

**Version**: 2.0.0
**Framework**: .NET 8.0
**Ultima actualizacion**: Marzo 2026
