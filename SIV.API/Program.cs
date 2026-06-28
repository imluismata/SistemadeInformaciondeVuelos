using Microsoft.EntityFrameworkCore;
using SIV.Infrastructure;
using SIV.Infrastructure.Repositories;
using SIV.Modules.Catalogo.Application;
using SIV.Modules.Vuelos.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<SivDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IVueloRepository, VueloRepository>();
builder.Services.AddScoped<IVueloService, VueloService>();
builder.Services.AddScoped<ICatalogoRepository, CatalogoRepository>();
builder.Services.AddScoped<ICatalogoService, CatalogoService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<SivDbContext>();
    db.Database.Migrate();
}

app.UseMiddleware<SIV.API.Middleware.ExceptionMiddleware>();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
