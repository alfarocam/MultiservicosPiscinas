using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Data;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Repositories;
using MultiserviciosPiscinas.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// --- Servicios personalizados ---
QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddControllersWithViews(); // Mantenemos solo uno aquo

// Registrar el HttpClient para el Seeder de Costa Rica
builder.Services.AddHttpClient<DivisionTerritorialSeeder>();

// Obtener cadena de conexion de appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<PiscinasYMultiserviciosContext>(options =>
    options.UseSqlServer(connectionString, sql => sql.CommandTimeout(30)));

// Pagina de errores para migraciones en desarrollo
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Agregar soporte para Razor Pages
builder.Services.AddRazorPages();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
});

// Configuracion de Autenticacion por Cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/InicioSesion"; // Ruta de redireccion si no esta autenticado
        options.ExpireTimeSpan = TimeSpan.FromMinutes(50); // Tiempo de vida de la sesion
    });

// Inyeccion de dependencias (Servicios y Repositorios)
builder.Services.AddTransient<Generales>();

// Repositorios  HU-2.5 - HU-3.4 - HU-7.1 - HU-6.5 - Dashboard
builder.Services.AddScoped<IDashboardRepository, DashboardRepository>();
builder.Services.AddScoped<IHistorialServicioRepository, HistorialServicioRepository>();
builder.Services.AddScoped<IReporteSatisfaccionRepository, ReporteSatisfaccionRepository>();
builder.Services.AddScoped<IReporteGastosOperativosRepository, ReporteGastosOperativosRepository>();
builder.Services.AddScoped<IReportesGeneralesRepository, ReportesGeneralesRepository>();
builder.Services.AddScoped<BitacoraService>();
builder.Services.AddScoped<GastoOperativoExcelService>();
builder.Services.AddScoped<ReportesGeneralesExcelService>();

// Cotización Manual
builder.Services.AddScoped<ICotizacionRepository, CotizacionRepository>();
builder.Services.AddScoped<CotizacionPdfService>();

// Carrito de cliente
builder.Services.AddScoped<ICarritoRepository, CarritoRepository>();

// Facturación
builder.Services.AddScoped<IFacturaRepository, FacturaRepository>();
builder.Services.AddScoped<FacturaPdfService>();

// Recomendaciones
builder.Services.AddScoped<IRecomendacionService, RecomendacionService>();

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();


if (app.Environment.IsDevelopment())
{

    app.UseDeveloperExceptionPage();
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

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=InicioSesion}/{id?}");

app.MapRazorPages();

using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DivisionTerritorialSeeder>();
    await seeder.SeedAsync();
}

app.Run();