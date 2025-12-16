# Configuración de PostgreSQL - appsettings.json

## ⚠️ IMPORTANTE: Actualizar manualmente

El archivo `appsettings.json` necesita la siguiente configuración para PostgreSQL.

**Copiar y pegar este contenido en tu archivo `appsettings.json`:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Port=5432;Database=FinanzasPersonalesDb;User Id=postgres;Password=postgres;"
  },
  "Jwt": {
    "Key": "TU_CLAVE_SECRETA_MUY_LARGA_Y_SEGURA_PARA_FINANZAS_PERSONALES_2025",
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

## 📝 Notas Importantes

1. **Password**: Si configuraste una contraseña diferente durante la instalación de PostgreSQL, cámbiala en `Password=postgres;`

2. **User Id**: El usuario por defecto es `postgres`. Si creaste otro usuario, cámbialo aquí.

3. **Port**: El puerto por defecto es `5432`. Si cambiaste el puerto durante la instalación, actualízalo.

##  Alternativas de Cadena de Conexión

Si la primera no funciona, prueba con esta sintaxis alternativa:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FinanzasPersonalesDb;Username=postgres;Password=postgres"
  }
}
```

## ✅ Después de Actualizar

Ejecuta estos comandos:

```bash
cd FinanzasPersonales.Api
dotnet ef database update --no-build
```

Si da error, ejecuta:

```bash
dotnet build
dotnet ef database update
```
