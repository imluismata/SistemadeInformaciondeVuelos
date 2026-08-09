using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SIV.API.Auth;
using SIV.API.Seguridad;
using SIV.Infrastructure;
using SIV.Modules;
using SIV.Modules.Auditoria.Application;
using SIV.Modules.Catalogo.Application;
using SIV.Modules.Eventos;
using SIV.Modules.Reportes.Application;
using SIV.Modules.Usuarios.Application.Dtos;
using SIV.Modules.Usuarios.Application.Interfaces;
using SIV.Modules.Usuarios.Domain;
using SIV.Modules.Vuelos.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    // Permite enviar el token JWT desde Swagger con el botón "Authorize".
    var esquema = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingresa el token JWT (sin la palabra 'Bearer').",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", esquema);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [esquema] = Array.Empty<string>() });
});

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddVuelosModule();
builder.Services.AddCatalogoModule();
builder.Services.AddAuditoriaModule();
builder.Services.AddReportesModule();
builder.Services.AddModules();
builder.Services.AddEventos();

// Prendo el chequeo de salud. Expone /health para saber si la app y su base de
// datos estan respondiendo (util para monitoreo y disponibilidad).
builder.Services.AddHealthChecks()
    .AddCheck<SIV.API.Health.BaseDatosHealthCheck>("base-datos");

// --- Autenticación y autorización con JWT ---
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.Seccion));
builder.Services.AddScoped<IProveedorTokenJwt, ProveedorTokenJwt>();

// Usuario actual: la auditoría lee el actor del JWT sin acoplarse al HttpContext.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<SIV.Shared.Contracts.IUsuarioActual, UsuarioActualHttp>();

var jwt = builder.Configuration.GetSection(JwtSettings.Seccion).Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Clave))
        };
    });

builder.Services.AddAuthorization();

// Límite de intentos por IP en login, códigos y escritura anónima (fuerza bruta).
builder.Services.AddLimitesDePeticiones();

builder.Services.AddCors(options =>
{
    options.AddPolicy("PortalPublico", policy =>
        policy.WithOrigins("http://localhost:4200", "http://localhost:4201", "http://localhost:7790")
            .AllowAnyHeader()
            .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SIV.Infrastructure.SivDbContext>();
    db.Database.Migrate();

    // Bootstrap: siembra el administrador inicial desde configuración (no en código).
    // Es idempotente, así que puede correr en cada arranque sin duplicar la cuenta.
    // Los valores reales viven en appsettings.Development.json / variables de entorno,
    // nunca en el repositorio. Si no hay Email/Password configurados, no se siembra.
    var adminCfg = app.Configuration.GetSection("AdminInicial");
    var adminEmail = adminCfg["Email"];
    var adminPassword = adminCfg["Password"];
    if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
    {
        var usuarios = scope.ServiceProvider.GetRequiredService<IUsuarioService>();
        await usuarios.AsegurarAdminInicialAsync(new CrearUsuarioInternoDto(
            adminCfg["Nombre"] ?? "Administrador SIV", adminEmail, adminPassword, RolUsuario.Administrador));
    }
}

app.UseMiddleware<SIV.API.Middleware.ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("PortalPublico");
// Va después de CORS para que la respuesta 429 también lleve sus cabeceras y el
// portal pueda leer el motivo en vez de un error opaco de red.
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
