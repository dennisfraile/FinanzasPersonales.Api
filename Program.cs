using FinanzasPersonales.Api.Data;
using FinanzasPersonales.Api.Jobs;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Hangfire;
using Hangfire.PostgreSql;

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
    // Opciones de contraseña (las hacemos flexibles para desarrollo)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6; // Contraseña de mínimo 6 caracteres
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

// Registrar job de notificaciones
builder.Services.AddScoped<NotificacionesJob>();

// Configurar Hangfire con PostgreSQL
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(c => c.UseNpgsqlConnection(connectionString)));

// Agregar servidor de Hangfire
builder.Services.AddHangfireServer();

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? ""))
    };
});

var app = builder.Build();

app.UseAuthentication(); // 1. Verifica quién eres (autenticación)
app.UseAuthorization();  // 2. Verifica qué permisos tienes (autorización)

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    // Habilitar dashboard de Hangfire en desarrollo
    var dashboardEnabled = builder.Configuration.GetValue<bool>("HangfireSettings:DashboardEnabled", true);
    if (dashboardEnabled)
    {
        app.UseHangfireDashboard("/hangfire");
    }
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Programar job recurrente de notificaciones (se ejecuta diariamente a las 9:00 AM)
RecurringJob.AddOrUpdate<NotificacionesJob>(
    "verificar-alertas-diarias",
    job => job.EjecutarVerificacionesAsync(),
    Cron.Daily(9) // 9:00 AM todos los días
);

app.Run();
