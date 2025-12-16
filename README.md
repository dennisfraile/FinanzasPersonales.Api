# Finanzas Personales - Backend API

API REST desarrollada con .NET 8 para el sistema de gestión de finanzas personales. Proporciona endpoints seguros para administrar gastos, ingresos, presupuestos, metas financieras y generar reportes.

## 🚀 Tecnologías

- **.NET 8** - Framework principal
- **ASP.NET Core Web API** - API REST
- **Entity Framework Core 8** - ORM
- **PostgreSQL** - Base de datos
- **ASP.NET Core Identity** - Autenticación y gestión de usuarios
- **JWT (JSON Web Tokens)** - Autenticación basada en tokens
- **EPPlus** - Exportación a Excel
- **Swagger/OpenAPI** - Documentación de API

## 📁 Estructura del Proyecto

```
FinanzasPersonales.Api/
├── Controllers/          # Endpoints de la API
│   ├── AuthController.cs           # Autenticación y perfil de usuario
│   ├── GastosController.cs         # CRUD de gastos
│   ├── IngresosController.cs       # CRUD de ingresos
│   ├── PresupuestosController.cs   # CRUD de presupuestos
│   ├── MetasController.cs          # CRUD de metas
│   ├── CategoriasController.cs     # CRUD de categorías
│   ├── DashboardController.cs      # Datos para dashboard y gráficas
│   └── ReportesController.cs       # Generación de reportes y exportación
├── Models/              # Entidades del dominio
│   ├── Gasto.cs
│   ├── Ingreso.cs
│   ├── Presupuesto.cs
│   ├── Meta.cs
│   └── Categoria.cs
├── Dtos/                # Data Transfer Objects
│   ├── AuthDtos.cs
│   ├── GastoDto.cs
│   ├── IngresoDto.cs
│   ├── PresupuestoDto.cs
│   ├── MetaDto.cs
│   ├── CategoriaDto.cs
│   ├── DashboardDto.cs
│   └── UserProfileDto.cs
├── Data/                # Contexto de base de datos
│   └── FinanzasDbContext.cs
├── Services/            # Servicios de negocio
│   └── ExportService.cs
├── Migrations/          # Migraciones de EF Core
└── Program.cs           # Configuración de la aplicación
```

## 📋 Funcionalidades

### Autenticación y Usuarios
- ✅ Registro de nuevos usuarios
- ✅ Login con JWT
- ✅ Gestión de perfil de usuario
- ✅ Cambio de contraseña

### Gestión Financiera
- ✅ **Gastos**: CRUD completo con categorización y tipo (Fijo/Variable)
- ✅ **Ingresos**: CRUD completo con categorización
- ✅ **Presupuestos**: Definición de límites por categoría con seguimiento
- ✅ **Metas**: Seguimiento de objetivos de ahorro
- ✅ **Categorías**: Gestión de categorías de gastos e ingresos

### Dashboard y Reportes
- ✅ Resumen mensual de finanzas
- ✅ Gráficas de ingresos vs gastos
- ✅ Distribución de gastos por categoría
- ✅ Progreso de metas
- ✅ Exportación a Excel

## 🔐 Endpoints Principales

### Auth
- `POST /api/Auth/register` - Registrar usuario
- `POST /api/Auth/login` - Iniciar sesión
- `GET /api/Auth/profile` - Obtener perfil
- `PUT /api/Auth/profile` - Actualizar perfil
- `PUT /api/Auth/change-password` - Cambiar contraseña

### Gastos
- `GET /api/Gastos` - Listar gastos (con paginación y filtros)
- `POST /api/Gastos` - Crear gasto
- `PUT /api/Gastos/{id}` - Actualizar gasto
- `DELETE /api/Gastos/{id}` - Eliminar gasto

### Ingresos
- `GET /api/Ingresos` - Listar ingresos (con paginación y filtros)
- `POST /api/Ingresos` - Crear ingreso
- `PUT /api/Ingresos/{id}` - Actualizar ingreso
- `DELETE /api/Ingresos/{id}` - Eliminar ingreso

### Presupuestos
- `GET /api/Presupuestos` - Listar presupuestos
- `POST /api/Presupuestos` - Crear presupuesto
- `PUT /api/Presupuestos/{id}` - Actualizar presupuesto
- `DELETE /api/Presupuestos/{id}` - Eliminar presupuesto

### Dashboard
- `GET /api/Dashboard?mes={mes}&ano={ano}` - Resumen del dashboard
- `GET /api/Dashboard/grafica/ingresos-vs-gastos` - Gráfica comparativa
- `GET /api/Dashboard/grafica/gastos-por-categoria` - Distribución de gastos

### Reportes
- `GET /api/Reportes/excel` - Exportar datos a Excel

## ⚙️ Configuración

### Variables de Entorno (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=finanzas_db;Username=postgres;Password=tu_password"
  },
  "Jwt": {
    "Key": "tu_clave_secreta_muy_segura_minimo_32_caracteres",
    "Issuer": "FinanzasPersonalesAPI",
    "Audience": "FinanzasPersonalesApp"
  }
}
```

## 🛠️ Instalación y Ejecución

### Prerrequisitos
- .NET 8 SDK
- PostgreSQL 12+

### Paso 1: Clonar el repositorio
```bash
cd FinanzasPersonales.Api
```

### Paso 2: Configurar la base de datos
1. Crear base de datos en PostgreSQL:
```sql
CREATE DATABASE finanzas_db;
```

2. Actualizar `appsettings.json` con tus credenciales

### Paso 3: Ejecutar migraciones
```bash
dotnet ef database update
```

### Paso 4: Ejecutar la aplicación
```bash
dotnet run
```

La API estará disponible en:
- HTTP: `http://localhost:5030`
- HTTPS: `https://localhost:7173`
- Swagger: `http://localhost:5030/swagger`

## 📊 Migraciones de Base de Datos

### Crear una nueva migración
```bash
dotnet ef migrations add NombreDeLaMigracion
```

### Aplicar migraciones
```bash
dotnet ef database update
```

### Revertir última migración
```bash
dotnet ef database update NombreMigracionAnterior
```

### Eliminar última migración
```bash
dotnet ef migrations remove
```

## 🔒 Seguridad

- **Autenticación**: JWT con expiración de 24 horas
- **Autorización**: Todos los endpoints requieren token excepto login/registro
- **Validación**: Validación de modelos con Data Annotations
- **CORS**: Configurado para permitir origen del frontend
- **Contraseñas**: Hash con ASP.NET Core Identity

## 📝 Notas Importantes

1. **DateTime UTC**: Todas las fechas se manejan en UTC para compatibilidad con PostgreSQL
2. **Paginación**: Los endpoints de listado soportan paginación con parámetros `pagina` y `tamañoPagina`
3. **Filtros**: Soportan filtros por mes, año, categoría y tipo
4. **Soft Delete**: Actualmente se usa eliminación física (futuro: implementar soft delete)

## 🐛 Troubleshooting

### Error: "no existe la columna"
Ejecutar migraciones pendientes:
```bash
dotnet ef database update
```

### Error: "DateTime UTC"
Asegurarse de que todas las fechas se convierten a UTC antes de guardar en PostgreSQL

### Error de conexión a PostgreSQL
Verificar:
1. PostgreSQL está corriendo
2. Credenciales en `appsettings.json` son correctas
3. Base de datos existe

## 📚 Documentación Adicional

- Swagger UI disponible en `/swagger` cuando la app está corriendo
- [Documentación oficial de .NET](https://docs.microsoft.com/dotnet/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [ASP.NET Core Identity](https://docs.microsoft.com/aspnet/core/security/authentication/identity)

## 👨‍💻 Desarrollo

### Agregar un nuevo endpoint
1. Crear DTO en `/Dtos`
2. Crear controlador en `/Controllers`
3. Agregar validaciones necesarias
4. Documentar con XML comments

### Agregar una nueva entidad
1. Crear modelo en `/Models`
2. Agregar DbSet en `FinanzasDbContext.cs`
3. Crear migración: `dotnet ef migrations add AgregarEntidadX`
4. Aplicar migración: `dotnet ef database update`

---

**Versión**: 1.0.0  
**Última actualización**: Diciembre 2025
