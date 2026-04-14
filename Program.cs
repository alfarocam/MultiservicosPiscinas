using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MultiservicosPiscinas.Data;

var builder = WebApplication.CreateBuilder(args);

// Obtener cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Configurar DbContext con SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Página de errores para migraciones en desarrollo
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Configurar Identity con roles
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Agregar soporte para MVC
builder.Services.AddControllersWithViews();

// Agregar soporte para Razor Pages
builder.Services.AddRazorPages();

var app = builder.Build();

// Crear roles y asegurar usuario admin con rol Administrador
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    string[] roles = { "Administrador", "Tecnico", "Cliente" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    var adminEmail = "admin@test.com";
    var adminPassword = "Admin123!";

    var user = await userManager.FindByEmailAsync(adminEmail);

    if (user == null)
    {
        user = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, adminPassword);

        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, "Administrador");
        }
    }
    else
    {
        if (!await userManager.IsInRoleAsync(user, "Administrador"))
        {
            await userManager.AddToRoleAsync(user, "Administrador");
        }
    }
}

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
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapear Razor Pages
app.MapRazorPages();

app.Run();