using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Data;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Repositories;
using MultiserviciosPiscinas.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Servicios personalizados --- 
builder.Services.AddControllersWithViews(); // Mantenemos solo uno aquí

// Registrar el HttpClient para el Seeder de Costa Rica
builder.Services.AddHttpClient<DivisionTerritorialSeeder>();

// Obtener cadena de conexión de appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<PiscinasYMultiserviciosContext>(options =>
    options.UseSqlServer(connectionString));

// Página de errores para migraciones en desarrollo
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Agregar soporte para Razor Pages
builder.Services.AddRazorPages();

// Configuración de Autenticación por Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/InicioSesion"; // Ruta de redirección si no está autenticado
        options.ExpireTimeSpan = TimeSpan.FromMinutes(50); // Tiempo de vida de la sesión
    });

// Inyección de dependencias (Servicios y Repositorios)
builder.Services.AddTransient<Generales>();

// Repositorios Camila — HU-2.5 y HU-3.4
builder.Services.AddScoped<IHistorialServicioRepository, HistorialServicioRepository>();
builder.Services.AddScoped<IReporteSatisfaccionRepository, ReporteSatisfaccionRepository>();

var app = builder.Build();

// Configuración del pipeline HTTP (Middlewares)
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

// Ruta por defecto MVC (Apunta a tu Login)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=InicioSesion}/{id?}");

// Mapear Razor Pages
app.MapRazorPages();

// EJECUTAR EL SEEDER
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DivisionTerritorialSeeder>();
    await seeder.SeedAsync();
}
// FIN DEL SEEDER

app.Run();