using Microsoft.EntityFrameworkCore;
using SIV.Infrastructure;
using SIV.Modules.Auditoria.Application;
using SIV.Modules.Catalogo.Application;
using SIV.Modules.Vuelos.Application;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddVuelosModule();
builder.Services.AddCatalogoModule();
builder.Services.AddAuditoriaModule();

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
app.MapControllers();

app.Run();
