using FinanzasPersonales.Api.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // 1. A�adimos la definici�n de seguridad
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http, // Usamos autenticaci�n HTTP
        Scheme = "Bearer",             // El esquema es "Bearer"
        BearerFormat = "JWT",          // El formato es JWT
        In = ParameterLocation.Header, // El token ir� en la cabecera
        Description = "Por favor, introduce tu token JWT con el prefijo Bearer. Ejemplo: 'Bearer eyJhbGciOi...'"
    });

    // 2. A�adimos el requisito de seguridad
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

// A�adir servicios de EF Core y SQL Server
builder.Services.AddDbContext<FinanzasDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Configurar ASP.NET Core Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
{
    // Opciones de contrase�a (las hacemos flexibles para desarrollo)
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6; // Contrase�a de m�nimo 6 caracteres
})
.AddEntityFrameworkStores<FinanzasDbContext>() // Le dice a Identity que use nuestro DbContext
.AddDefaultTokenProviders(); // A�ade los proveedores para generar tokens (ej. reseteo de contrase�a)

// Configurar Autenticaci�n y JWT Bearer
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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

app.UseAuthentication(); // 1. Verifica qui�n eres (autenticaci�n)
app.UseAuthorization();  // 2. Verifica qu� permisos tienes (autorizaci�n)

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

