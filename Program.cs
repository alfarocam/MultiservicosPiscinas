using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Interfaces;     
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Repositories; 
using MultiserviciosPiscinas.Services;

var builder = WebApplication.CreateBuilder(args);

// Obtener cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<PiscinasYMultiserviciosContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Página de errores para migraciones en desarrollo
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Agregar soporte para MVC
builder.Services.AddControllersWithViews();

// Agregar soporte para Razor Pages
builder.Services.AddRazorPages();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/InicioSesion"; //ruta a donde redirige si no está autenticado
        options.ExpireTimeSpan = TimeSpan.FromMinutes(50); //tiempo de vida de la sesión
    });

builder.Services.AddTransient<Generales>();

// Repositorios Camila — HU-2.5 y HU-3.4
builder.Services.AddScoped<IHistorialServicioRepository, HistorialServicioRepository>();
builder.Services.AddScoped<IReporteSatisfaccionRepository, ReporteSatisfaccionRepository>();

var app = builder.Build();

// Configuración del pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

// Ruta por defecto MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=InicioSesion}/{id?}");

// Mapear Razor Pages
app.MapRazorPages();

app.Run();
