using Microsoft.AspNetCore.Authentication.Cookies;
using SIV.Intranet.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Acceso al HttpContext para leer el token del usuario en el TokenHandler.
builder.Services.AddHttpContextAccessor();

// --- Autenticación por cookie de la propia intranet ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.AccessDeniedPath = "/Cuenta/Denegado";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    });

builder.Services.AddAuthorization();

// --- Clientes HTTP tipados hacia la API del SIV ---
// El TokenHandler adjunta el JWT del usuario a cada llamada.
builder.Services.AddTransient<TokenHandler>();

var urlApi = new Uri(builder.Configuration["SivApi:BaseUrl"]!);
void ConfigurarCliente(HttpClient client) => client.BaseAddress = urlApi;

builder.Services.AddHttpClient<IAutenticacionApi, AutenticacionApi>(ConfigurarCliente)
    .AddHttpMessageHandler<TokenHandler>();
builder.Services.AddHttpClient<ICatalogoApi, CatalogoApi>(ConfigurarCliente)
    .AddHttpMessageHandler<TokenHandler>();
builder.Services.AddHttpClient<IVuelosApi, VuelosApi>(ConfigurarCliente)
    .AddHttpMessageHandler<TokenHandler>();
builder.Services.AddHttpClient<IAuditoriaApi, AuditoriaApiClient>(ConfigurarCliente)
    .AddHttpMessageHandler<TokenHandler>();
builder.Services.AddHttpClient<IUsuariosApi, UsuariosApiClient>(ConfigurarCliente)
    .AddHttpMessageHandler<TokenHandler>();
builder.Services.AddHttpClient<IReportesApi, ReportesApiClient>(ConfigurarCliente)
    .AddHttpMessageHandler<TokenHandler>();
builder.Services.AddHttpClient<IActividadApi, ActividadApiClient>(ConfigurarCliente)
    .AddHttpMessageHandler<TokenHandler>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Vuelos}/{action=Index}/{id?}");

app.Run();
