using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SIV.API.Auth;
using SIV.Infrastructure;
using SIV.Modules;
using SIV.Modules.Auditoria.Application;
using SIV.Modules.Catalogo.Application;
using SIV.Modules.Eventos;
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
builder.Services.AddModules();
builder.Services.AddEventos();

// --- Autenticación y autorización con JWT ---
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.Seccion));
builder.Services.AddScoped<IProveedorTokenJwt, ProveedorTokenJwt>();

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

builder.Services.AddCors(options =>
{
    options.AddPolicy("PortalPublico", policy =>
        policy.WithOrigins("http://localhost:4200")
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
}

app.UseMiddleware<SIV.API.Middleware.ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("PortalPublico");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
