# Configuración de appsettings.json para Módulo 2 (Notificaciones)

Por favor, agrega las siguientes secciones a tu archivo `appsettings.json`:

## 1. Configuración de Email (SMTP)

```json
{
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "dennisfraile3@gmail.com",
    "SenderName": "Finanzas Personales",
    "Username": "dennisfraile3@gmail.com",
    "Password": "",
    "EnableSsl": true
  }
}
```

### Notas sobre Email:
- **Para Gmail**: Necesitas generar una "Contraseña de Aplicación" desde tu cuenta de Google
  - Ve a: https://myaccount.google.com/apppasswords
  - Genera una contraseña específica para esta app
  - Usa esa contraseña en el campo `Password`

- **Para otros proveedores**:
  - **Outlook/Hotmail**: `smtp.office365.com`, puerto `587`
  - **Yahoo**: `smtp.mail.yahoo.com`, puerto `587`
  - **Otros**: Consulta la documentación de tu proveedor

## 2. Configuración de Hangfire

```json
{
  "HangfireSettings": {
    "DashboardEnabled": true,
    "DashboardPath": "/hangfire"
  }
}
```

### Notas sobre Hangfire:
- El dashboard estará disponible en: `https://localhost:5030/hangfire`
- Puedes ver jobs programados, ejecutados, y resultados
- **IMPORTANTE**: En producción, protege el dashboard con autenticación

## Ejemplo completo de appsettings.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Hangfire": "Information"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=FinanzasPersonalesDb;Username=postgres;Password=1234"
  },
  "Jwt": {
    "Key": "tu-clave-secreta-super-segura-de-al-menos-32-caracteres",
    "Issuer": "FinanzasPersonales.Api",
    "Audience": "FinanzasPersonales.Api",
    "ExpireMinutes": 60
  },
  "EmailSettings": {
    "SmtpServer": "smtp.gmail.com",
    "SmtpPort": 587,
    "SenderEmail": "tu-email@gmail.com",
    "SenderName": "Finanzas Personales",
    "Username": "tu-email@gmail.com",
    "Password": "tu-contraseña-de-aplicacion",
    "EnableSsl": true
  },
  "HangfireSettings": {
    "DashboardEnabled": true,
    "DashboardPath": "/hangfire"
  }
}
```

## Pasos para configurar:

1. Abre tu archivo `appsettings.json`
2. Agrega las secciones `EmailSettings` y `HangfireSettings`
3. Completa los datos de tu email SMTP
4. Guarda el archivo
5. Reinicia la aplicación

**Nota**: También deberías actualizar `appsettings.Development.json` con la misma configuración para desarrollo.
