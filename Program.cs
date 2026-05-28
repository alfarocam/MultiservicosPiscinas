using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiservicosPiscinas.Models;
//using MultiservicosPiscinas.Data;

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

// Ruta por defecto MVC
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}");

// Mapear Razor Pages
app.MapRazorPages();

app.Run();