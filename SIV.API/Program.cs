using SIV.Infrastructure;
using SIV.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter()));
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddModules();

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

app.UseMiddleware<SIV.API.Middleware.ExceptionMiddleware>();
app.UseHttpsRedirection();
app.UseCors("PortalPublico");
app.MapControllers();

app.Run();
