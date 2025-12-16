# Configuración Necesaria para Migraciones

## ⚠️ ACCIÓN REQUERIDA

El comando `dotnet ef migrations add InitialCreate` falla porque EF Tools no puede crear el DbContext.

## 📋 Solución

Verifica que tu archivo `appsettings.json` en la carpeta `FinanzasPersonales.Api` contenga:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FinanzasPersonalesDb;Username=postgres;Password=1234"
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

## 🔧 Pasos

1. Abre el archivo:  
   `c:\Users\LENOVO\Documents\Trabajo\FinanzasPersonales.Api\FinanzasPersonales.Api\appsettings.json`

2. Si no existe o está vacío, copia y pega el JSON de arriba

3. Guarda el archivo

4. Ejecuta manualmente:
   ```bash
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   ```

## ✅ Estado Actual

- Código: ✅ **Compila perfectamente**
- Errores: ✅ **Todos corregidos**
- Configuración: ⏳ **Necesita verificación manual**
