using Microsoft.AspNetCore.Authentication.Cookies;
using SIV.Intranet.Services;

var builder = WebApplication.CreateBuilder(args);

// El filtro muestra una página amigable si la API no está disponible (caso de la
// práctica "API no disponible") en vez de un 500 crudo.
builder.Services.AddControllersWithViews(opciones =>
    opciones.Filters.Add<SIV.Intranet.Filters.ApiNoDisponibleFilter>());

// Acceso al HttpContext para leer el token del usuario en el TokenHandler.
builder.Services.AddHttpContextAccessor();

// --- Autenticación por cookie de la propia intranet ---
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.AccessDeniedPath = "/Error/403";
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

// Páginas de error con marca (500, 404, 403…) activas también en Development para
// que la demo muestre siempre las páginas amigables en vez del stack trace crudo.
app.UseExceptionHandler("/Error/500");
app.UseStatusCodePagesWithReExecute("/Error/{0}");

if (!app.Environment.IsDevelopment())
{
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
