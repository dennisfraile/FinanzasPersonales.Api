# Guía de Migración: SQL Server → PostgreSQL

## 📋 Resumen

Esta guía documenta cómo migrar tu API de finanzas personales de SQL Server a PostgreSQL.

---

## ✅ Cambios Realizados

### 1. **Paquete NuGet Actualizado**

**Archivo**: `FinanzasPersonales.Api.csproj`

```xml
<!-- ANTES -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.10" />

<!-- DESPUÉS -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
```

### 2. **Provider en Program.cs**

**Archivo**: `Program.cs`

```csharp
// ANTES
builder.Services.AddDbContext<FinanzasDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// DESPUÉS  
builder.Services.AddDbContext<FinanzasDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
```

### 3. **Cadena de Conexión**

**Archivo**: `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FinanzasPersonalesDb;Username=postgres;Password=tu_password"
  }
}
```

**Para producción** (ejemplo con base de datos en la nube):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=tu-servidor.postgres.database.azure.com;Database=finanzasdb;Username=admin@tu-servidor;Password=TuPassword123!;SslMode=Require"
  }
}
```

---

## 🚀 Pasos para Migrar

### 1. **Instalar PostgreSQL**

#### Opción A: Local (Desarrollo)
- Descargar desde: https://www.postgresql.org/download/
- Instalar con pgAdmin (interfaz gráfica incluida)
- Puerto por defecto: 5432
- Usuario por defecto: postgres

#### Opción B: Docker (Recomendado para desarrollo)
```bash
docker run --name postgres-finanzas -e POSTGRES_PASSWORD=mipassword -p 5432:5432 -d postgres:16
```

#### Opción C: Cloud (Producción)
- **Azure Database for PostgreSQL** (gratis con créditos Azure)
- **AWS RDS PostgreSQL** (free tier disponible)
- **Google Cloud SQL PostgreSQL**
- **Supabase** (gratis hasta 500MB)
- **Neon** (serverless PostgreSQL gratuito)
- **Render** (free tier con PostgreSQL)

### 2. **Actualizar Dependencias**

```bash
cd FinanzasPersonales.Api

# Restaurar paquetes NuGet
dotnet restore

# Verificar que Npgsql esté instalado
dotnet list package
```

### 3. **Actualizar Cadena de Conexión**

Edita `appsettings.json` o `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FinanzasPersonalesDb;Username=postgres;Password=TU_PASSWORD_AQUI"
  }
}
```

### 4. **Borrar Migraciones Antiguas (SQL Server)**

```bash
# Eliminar carpeta de migraciones existentes
Remove-Item -Recurse -Force .\Migrations

# O en Linux/Mac
rm -rf Migrations/
```

### 5. **Crear Nuevas Migraciones (PostgreSQL)**

```bash
# Crear migración inicial
dotnet ef migrations add InitialCreate

# Revisar archivos generados en /Migrations
```

### 6. **Aplicar Migraciones a PostgreSQL**

```bash
# Crear la base de datos y aplicar schema
dotnet ef database update
```

### 7. **Verificar Conexión**

```bash
# Ejecutar la aplicación
dotnet run

# Acceder a Swagger
# https://localhost:5001/swagger
```

---

## 🔧 Configuración de appsettings.json Completa

### Desarrollo Local

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=FinanzasPersonalesDb;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Key": "TU_CLAVE_SECRETA_MUY_LARGA_Y_SEGURA_AQUI",
    "Issuer": "FinanzasPersonalesApi",
    "Audience": "FinanzasPersonalesClients"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Producción (con SSL)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=mi-db.region.rds.amazonaws.com;Port=5432;Database=finanzasdb;Username=admin;Password=${DB_PASSWORD};SslMode=Require;Trust Server Certificate=true"
  }
}
```

---

## 💰 Comparativa de Costos (Producción)

| Proveedor              | Plan Gratuito        | Costo Mensual (Básico) |
| ---------------------- | -------------------- | ---------------------- |
| **SQL Server Azure**   | ❌ No                 | ~$5-15 USD             |
| **PostgreSQL Azure**   | ✅ Sí (con créditos)  | ~$5 USD                |
| **AWS RDS PostgreSQL** | ✅ Sí (12 meses)      | ~$10-15 USD            |
| **Supabase**           | ✅ 500MB gratis       | $0-25 USD              |
| **Neon**               | ✅ 3GB gratis         | $0-19 USD              |
| **Render**             | ✅ 90 días gratis     | $0-7 USD               |
| **Railway**            | ✅ $5 crédito mensual | $0-5 USD               |

**Ahorro estimado**: 40-70% comparado con SQL Server en Azure.

---

## ⚠️ Diferencias Importantes

### 1. **Tipos de Datos**

PostgreSQL usa tipos diferentes, pero EF Core los maneja automáticamente:

| SQL Server      | PostgreSQL      | EF Core Mapping |
| --------------- | --------------- | --------------- |
| `NVARCHAR(MAX)` | `TEXT`          | Automático      |
| `DATETIME2`     | `TIMESTAMP`     | Automático      |
| `DECIMAL(18,2)` | `NUMERIC(18,2)` | Automático      |
| `BIT`           | `BOOLEAN`       | Automático      |
| `BIGINT`        | `BIGINT`        | Automático      |

### 2. **Sensibilidad a Mayúsculas/Minúsculas**

PostgreSQL es **case-sensitive** por defecto. Para búsquedas insensibles:

```csharp
// ANTES (SQL Server)
.Where(g => g.Descripcion.Contains(busqueda))

// DESPUÉS (PostgreSQL - case insensitive)
.Where(g => EF.Functions.ILike(g.Descripcion, $"%{busqueda}%"))
```

### 3. **Sintaxis de Cadenas de Conexión**

```
SQL Server:  Server=localhost;Database=MyDb;User Id=sa;Password=pass;
PostgreSQL:  Host=localhost;Database=MyDb;Username=postgres;Password=pass;
```

---

## 🐛 Troubleshooting

### Error: "Could not load file or assembly 'Npgsql'"

**Solución**:
```bash
dotnet clean
dotnet restore
dotnet build
```

### Error: "Connection refused" o "Can't connect"

**Verificar**:
1. PostgreSQL está corriendo: `pg_isready` (Linux/Mac) o Services (Windows)
2. Puerto correcto: `5432` por defecto
3. Firewall permite conexión
4. Contraseña correcta en cadena de conexión

### Error: "Database does not exist"

**Crear manualmente**:
```sql
CREATE DATABASE "FinanzasPersonalesDb";
```

O configurar auto-creación:
```csharp
// En Program.cs, después de builder.Build()
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanzasDbContext>();
    db.Database.Migrate(); // Crea la BD si no existe
}
```

### Error: "Role postgres does not exist"

**Crear usuario**:
```sql
CREATE ROLE postgres WITH LOGIN PASSWORD 'mipassword';
ALTER ROLE postgres CREATEDB;
```

---

## 📋 Checklist de Migración

- [ ] PostgreSQL instalado y corriendo
- [ ] Paquete NuGet cambiado a `Npgsql.EntityFrameworkCore.PostgreSQL`
- [ ] `Program.cs` usa `UseNpgsql` en lugar de `UseSqlServer`
- [ ] Cadena de conexión actualizada en `appsettings.json`
- [ ] Migraciones antiguas eliminadas
- [ ] Nueva migración creada con `dotnet ef migrations add`
- [ ] Base de datos creada con `dotnet ef database update`
- [ ] Aplicación arranca sin errores
- [ ] Endpoints funcionan en Swagger
- [ ] Autenticación funciona correctamente

---

## 🎯 Ventajas de PostgreSQL

✅ **Gratis y Open Source** - Sin costos de licencia  
✅ **Rendimiento excelente** - Especialmente en lecturas complejas  
✅ **Tipos de datos avanzados** - JSON, Arrays, JSONB  
✅ **Extensiones poderosas** - PostGIS para datos geográficos  
✅ **Comunidad activa** - Soporte y recursos abundantes  
✅ **Multiplataforma** - Windows, Linux, macOS, Docker  
✅ **Opciones cloud baratas** - Muchos proveedores con planes gratuitos  

---

## 🔗 Recursos Útiles

- **PostgreSQL Documentation**: https://www.postgresql.org/docs/
- **Npgsql Documentation**: https://www.npgsql.org/efcore/
- **pgAdmin (GUI)**: https://www.pgadmin.org/
- **DBeaver (Cliente universal)**: https://dbeaver.io/
- **Supabase (PostgreSQL managed)**: https://supabase.com/
- **Neon (Serverless PostgreSQL)**: https://neon.tech/

---

## 📞 Soporte

En caso de problemas, verificar:
1. Logs de la aplicación (`dotnet run`)
2. Logs de PostgreSQL (usualmente en `/var/log/postgresql/`)
3. Conexión con `psql` o pgAdmin

---

**¡Migración completa! Tu API ahora usa PostgreSQL y ahorrará costos en producción.** 🎉
