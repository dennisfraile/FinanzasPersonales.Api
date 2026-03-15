using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Jobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using Microsoft.OpenApi.Models;
using Hangfire;
using Hangfire.PostgreSql;
using Hangfire.Dashboard;
using FinanzasPersonales.Api.Hubs;
using System.Text.Json;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 1. Añadimos la definición de seguridad
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, // Usamos autenticación HTTP
        Scheme = "Bearer",             // El esquema es "Bearer"
        BearerFormat = "JWT",          // El formato es JWT
        In = ParameterLocation.Header, // El token irá en la cabecera
        Description = "Por favor, introduce tu token JWT con el prefijo Bearer. Ejemplo: 'Bearer eyJhbGciOi...'"
    });

    // 2. Añadimos el requisito de seguridad
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer" // Debe coincidir con el 'Id' de AddSecurityDefinition
                }
            },
            new string[] {}
        }
    });
});

// Añadir servicios de EF Core y PostgreSQL
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<FinanzasDbContext>(options =>
    options.UseNpgsql(connectionString));

// Configurar ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Política de contraseña segura
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 8;

    // Bloqueo de cuenta tras intentos fallidos
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // Permitir nombres de usuario no únicos (el email es lo que identifica al usuario)
    options.User.RequireUniqueEmail = true; // Email DEBE ser único

    // Permitir letras, números, espacios y caracteres especiales en el nombre de usuario
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+ áéíóúÁÉÍÓÚñÑ";
})
.AddEntityFrameworkStores<FinanzasDbContext>() // Le dice a Identity que use nuestro DbContext
.AddDefaultTokenProviders(); // Añade los proveedores para generar tokens (ej. reseteo de contraseña)

// Registrar servicios de exportación
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IExportService, FinanzasPersonales.Api.Services.ExportService>();

// Registrar servicios de notificaciones
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IEmailService, FinanzasPersonales.Api.Services.EmailService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.INotificacionService, FinanzasPersonales.Api.Services.NotificacionService>();

// Registrar servicio de metas mejoradas
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IMetasService, FinanzasPersonales.Api.Services.MetasService>();

// Registrar servicios de negocio (service layer)
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IGastosService, FinanzasPersonales.Api.Services.GastosService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IIngresosService, FinanzasPersonales.Api.Services.IngresosService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IPresupuestosService, FinanzasPersonales.Api.Services.PresupuestosService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IDashboardService, FinanzasPersonales.Api.Services.DashboardService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IReportesService, FinanzasPersonales.Api.Services.ReportesService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.ITransferenciasService, FinanzasPersonales.Api.Services.TransferenciasService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.ICuentasService, FinanzasPersonales.Api.Services.CuentasService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IGastosRecurrentesService, FinanzasPersonales.Api.Services.GastosRecurrentesService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IIngresosRecurrentesService, FinanzasPersonales.Api.Services.IngresosRecurrentesService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IReglasCategoriaService, FinanzasPersonales.Api.Services.ReglasCategoriaService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IImportacionCsvService, FinanzasPersonales.Api.Services.ImportacionCsvService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IPlantillasGastoService, FinanzasPersonales.Api.Services.PlantillasGastoService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IDeudasService, FinanzasPersonales.Api.Services.DeudasService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IGastosCompartidosService, FinanzasPersonales.Api.Services.GastosCompartidosService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.ITipoCambioService, FinanzasPersonales.Api.Services.TipoCambioService>();
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IReportesProgramadosService, FinanzasPersonales.Api.Services.ReportesProgramadosService>();

// Registrar servicio de almacenamiento de archivos
builder.Services.AddScoped<FinanzasPersonales.Api.Services.IFileStorageService, FinanzasPersonales.Api.Services.LocalFileStorageService>();

// Configurar CORS desde appsettings.json (restringido a métodos y headers específicos)
var allowedOrigins = builder.Configuration.GetSection("CorsSettings:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:5173" };
if (allowedOrigins.Length == 0 || allowedOrigins.Any(string.IsNullOrWhiteSpace))
    throw new InvalidOperationException("CORS: AllowedOrigins must contain at least one valid origin.");

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .WithHeaders("Content-Type", "Authorization", "X-Requested-With", "x-signalr-user-agent")
              .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
              .AllowCredentials();
    });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Límite global: 100 requests por minuto por IP
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Límite estricto para auth: 10 requests por minuto por IP
    options.AddPolicy("auth", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// Registrar jobs
builder.Services.AddScoped<NotificacionesJob>();
builder.Services.AddScoped<FinanzasPersonales.Api.Jobs.ReportesProgramadosJob>();
builder.Services.AddScoped<FinanzasPersonales.Api.Jobs.RecurrentesJob>();

// Configurar Hangfire con PostgreSQL
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));

// Agregar servidor de Hangfire
builder.Services.AddHangfireServer();

// Agregar SignalR
builder.Services.AddSignalR();

// Validar JWT Key al iniciar
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrWhiteSpace(jwtKey) || Encoding.UTF8.GetByteCount(jwtKey) < 32)
    throw new InvalidOperationException("JWT Key must be configured and at least 32 bytes long.");

// Configurar Autenticación y JWT Bearer
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        // CRÍTICO: Configurar el mapeo de claims
        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role
    };

    // Soporte para token de SignalR desde query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Limitar tamaño de request body (10MB máximo para uploads)
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 10_485_760; // 10MB
});

var app = builder.Build();

// Aplicar migraciones automáticamente en producción
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<FinanzasDbContext>();
    db.Database.Migrate();
}

// Middleware global de manejo de excepciones (evita exponer stack traces)
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();

        if (exceptionFeature != null)
        {
            logger.LogError(exceptionFeature.Error, "Unhandled exception");
        }

        await context.Response.WriteAsync(JsonSerializer.Serialize(new
        {
            message = "Ha ocurrido un error interno del servidor.",
            status = 500
        }));
    });
});

// HTTPS enforcement
app.UseHttpsRedirection();
app.UseHsts();

// Headers de seguridad
app.Use(async (context, next) =>
{
    context.Response.Headers["X-Content-Type-Options"] = "nosniff";
    context.Response.Headers["X-Frame-Options"] = "DENY";
    context.Response.Headers["X-XSS-Protection"] = "1; mode=block";
    context.Response.Headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
    context.Response.Headers["Content-Security-Policy"] = "default-src 'self'; frame-ancestors 'none'";
    context.Response.Headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
    await next();
});

// Habilitar CORS
app.UseCors();

// Rate limiting
app.UseRateLimiter();

app.UseAuthentication(); // 1. Verifica quién eres (autenticación)
app.UseAuthorization();  // 2. Verifica qué permisos tienes (autorización)

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Habilitar dashboard de Hangfire protegido con autenticación
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = new[] { new HangfireAuthorizationFilter() },
    IsReadOnlyFunc = _ => true
});

app.MapControllers();

// Mapear hub de SignalR para notificaciones en tiempo real
app.MapHub<NotificacionesHub>("/hubs/notificaciones");

// Programar job recurrente de notificaciones (se ejecuta diariamente a las 9:00 AM)
RecurringJob.AddOrUpdate<NotificacionesJob>(
    "verificar-alertas-diarias",
    job => job.EjecutarVerificacionesAsync(),
    Cron.Daily(9) // 9:00 AM todos los días
);

// Programar job de reportes programados (se ejecuta diariamente a las 7:00 AM)
RecurringJob.AddOrUpdate<FinanzasPersonales.Api.Jobs.ReportesProgramadosJob>(
    "enviar-reportes-programados",
    job => job.EjecutarEnvioReportesAsync(),
    Cron.Daily(7) // 7:00 AM todos los días
);

// Programar job de transacciones recurrentes (se ejecuta cada hora)
RecurringJob.AddOrUpdate<FinanzasPersonales.Api.Jobs.RecurrentesJob>(
    "generar-transacciones-recurrentes",
    job => job.GenerarTransaccionesRecurrentesAsync(),
    Cron.Hourly() // Cada hora para no perder pagos del día
);

app.Run();

/// <summary>
/// Filtro de autorización para Hangfire Dashboard - solo usuarios autenticados
/// </summary>
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true;
    }
}
