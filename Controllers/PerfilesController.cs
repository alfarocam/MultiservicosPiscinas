using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Data;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class PerfilesController(PiscinasYMultiserviciosContext _context, BitacoraService _bitacora) : Controller
    {

        //ver el perfil 
        //get
        //info asociada de roles y clientes
        public async Task<IActionResult> Detalle()
        {
            await DivisionTerritorialSeeder.AsegurarIntegridadRolesYDescripcionesAsync(_context);

            string? correoLogueado = User.Identity?.Name;

            if (string.IsNullOrEmpty(correoLogueado))
            {
                return Challenge();
            }

            var usuario = await _context.Usuario
                .Include(u => u.Rol)
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.TelefonosCliente)
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.DireccionCliente)
                        .ThenInclude(d => d.Distrito)
                            .ThenInclude(d => d.Canton)
                                .ThenInclude(c => c.Provincia)
                .FirstOrDefaultAsync(u => u.Correo == correoLogueado);

            if (usuario == null)
            {
                return await CerrarSesionPorCorreoInvalidoAsync();
            }

            usuario.Telefono = await ObtenerTelefonoUsuarioAsync(usuario);
            return View(usuario);
        }
        private async Task CargarListasDireccionAsync(Usuario usuario, int? distritoId = null)
        {
            var provincias = await _context.Provincia.OrderBy(p => p.Nombre).ToListAsync();
            ViewBag.Provincias = provincias;

            string detallesPrevios = ViewBag.DireccionDetalles as string ?? "";

            ViewBag.SelectedProvinciaId = 0;
            ViewBag.SelectedCantonId = 0;
            ViewBag.SelectedDistritoId = 0;
            ViewBag.DireccionDetalles = "";
            ViewBag.Cantones = new List<Canton>();
            ViewBag.Distritos = new List<Distrito>();

            if (usuario.Cliente != null)
            {
                int? targetDistritoId = distritoId;
                string? detalles = "";

                if (!targetDistritoId.HasValue)
                {
                    var direccionPrincipal = usuario.Cliente.DireccionCliente
                        .FirstOrDefault(d => d.EsPrincipal == 1);

                    if (direccionPrincipal != null)
                    {
                        targetDistritoId = direccionPrincipal.DistritoId;
                        detalles = direccionPrincipal.Detalles;
                    }
                }
                else
                {
                    detalles = detallesPrevios;
                }

                if (targetDistritoId.HasValue && targetDistritoId.Value > 0)
                {
                    var distritoObj = await _context.Distrito
                        .Include(d => d.Canton)
                        .FirstOrDefaultAsync(d => d.Id == targetDistritoId.Value);

                    if (distritoObj != null)
                    {
                        var cantonObj = distritoObj.Canton;
                        if (cantonObj != null)
                        {
                            ViewBag.SelectedProvinciaId = cantonObj.ProvinciaId;
                            ViewBag.SelectedCantonId = cantonObj.Id;
                            ViewBag.SelectedDistritoId = distritoObj.Id;
                            ViewBag.DireccionDetalles = detalles;

                            ViewBag.Cantones = await _context.Canton
                                .Where(c => c.ProvinciaId == cantonObj.ProvinciaId)
                                .OrderBy(c => c.Nombre)
                                .ToListAsync();

                            ViewBag.Distritos = await _context.Distrito
                                .Where(d => d.CantonId == cantonObj.Id)
                                .OrderBy(d => d.Nombre)
                                .ToListAsync();
                        }
                    }
                }
            }
        }

        //edit
        //
        //cargar el formulario
        public async Task<IActionResult> Editar()
        {
            string? correoLogueado = User.Identity?.Name;

            if (string.IsNullOrEmpty(correoLogueado))
            {
                return Challenge();
            }

            var usuario = await _context.Usuario
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.TelefonosCliente)
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.DireccionCliente)
                .FirstOrDefaultAsync(u => u.Correo == correoLogueado);

            if (usuario == null)
            {
                return await CerrarSesionPorCorreoInvalidoAsync();
            }

            usuario.Telefono = await ObtenerTelefonoUsuarioAsync(usuario);
            await CargarListasDireccionAsync(usuario);

            return View(usuario);
        }

        //guardar cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            string nombre,
            string apellidoPaterno,
            string apellidoMaterno,
            string correo,
            string? telefono, // solo existe para clientes; si fuera 'string' MVC lo exigiría a todos los roles
            int? distritoId,
            string? detalles)
        {
            var usuarioDb = await _context.Usuario
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.TelefonosCliente)
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.DireccionCliente)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuarioDb == null)
            {
                return NotFound();
            }

            nombre = (nombre ?? string.Empty).Trim();
            apellidoPaterno = (apellidoPaterno ?? string.Empty).Trim();
            apellidoMaterno = (apellidoMaterno ?? string.Empty).Trim();
            correo = (correo ?? string.Empty).Trim();
            telefono = telefono?.Trim();
            detalles = detalles?.Trim();

            // validaciones
            if (string.IsNullOrWhiteSpace(nombre))
            {
                ModelState.AddModelError("", "El nombre es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(apellidoPaterno))
            {
                ModelState.AddModelError("", "El primer apellido es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(apellidoMaterno))
            {
                ModelState.AddModelError("", "El segundo apellido es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                ModelState.AddModelError("", "El correo es obligatorio.");
            }
            else if (!Regex.IsMatch(correo, @"^[a-zA-Z0-9._%+-]+@(gmail\.com|hotmail\.com|yahoo\.com|outlook\.com|icloud\.com|live\.com|piscinas\.com|multiservicios\.com|test\.com|correo\.com|ejemplo\.com)$", RegexOptions.IgnoreCase))
            {
                ModelState.AddModelError("", "Ingresá un correo válido (@gmail.com, @hotmail.com, @yahoo.com, @outlook.com o institucional).");
            }
            else
            {
                // sin esta validación el SaveChanges falla con un mensaje genérico.
                bool correoEnUso = await _context.Usuario
                    .AnyAsync(u => u.Correo == correo && u.Id != id);

                if (correoEnUso)
                {
                    ModelState.AddModelError("", "El correo ya está registrado por otro usuario.");
                }
            }

            if (string.IsNullOrWhiteSpace(telefono))
            {
                ModelState.AddModelError("", "El teléfono es obligatorio.");
            }
            else if (!Generales.EsTelefonoValido(telefono))
            {
                ModelState.AddModelError("", "El teléfono debe contener 8 dígitos con el formato 8888-8888.");
            }
            else
            {
                var telefonoNormalizado = Generales.NormalizarTelefono(telefono);
                int? clienteActualId = usuarioDb.Cliente?.Id;
                bool telefonoEnUso = await _context.TelefonosCliente.AnyAsync(t =>
                    (!clienteActualId.HasValue || t.ClienteId != clienteActualId.Value) &&
                    t.NumeroTelefono.Trim().Replace("-", "").Replace(" ", "") == telefonoNormalizado);

                if (telefonoEnUso)
                {
                    ModelState.AddModelError("", "El teléfono ya está registrado por otro cliente.");
                }
            }

            if (usuarioDb.Cliente != null)
            {
                if (!distritoId.HasValue || distritoId.Value <= 0)
                {
                    ModelState.AddModelError("", "Debe seleccionar una provincia, cantón y distrito válidos.");
                }

                if (string.IsNullOrWhiteSpace(detalles))
                {
                    ModelState.AddModelError("", "Los detalles de la dirección son obligatorios.");
                }
            }

            if (!ModelState.IsValid)
            {
                usuarioDb.Telefono = telefono;
                ViewBag.DireccionDetalles = detalles;
                await CargarListasDireccionAsync(usuarioDb, distritoId);
                return View(usuarioDb);
            }

            var telefonoFormateado = Generales.FormatearTelefono(telefono);
            var telefonoAnterior = await ObtenerTelefonoUsuarioAsync(usuarioDb);
            var valorAnterior = $"Nombre: {usuarioDb.Nombre} {usuarioDb.ApellidoPaterno} {usuarioDb.ApellidoMaterno} | Correo: {usuarioDb.Correo} | Teléfono: {telefonoAnterior ?? "N/A"}";

            // act usuario
            usuarioDb.Nombre = nombre;
            usuarioDb.ApellidoPaterno = apellidoPaterno;
            usuarioDb.ApellidoMaterno = apellidoMaterno;
            usuarioDb.Correo = correo;

            // actualiza tel
            if (usuarioDb.Cliente != null)
            {
                var telefonoPrincipal = usuarioDb.Cliente.TelefonosCliente
                    .FirstOrDefault(t => t.EsPrincipal == 1);

                if (telefonoPrincipal != null)
                {
                    telefonoPrincipal.NumeroTelefono = telefonoFormateado;
                    _context.TelefonosCliente.Update(telefonoPrincipal);
                }
                else
                {
                    var nuevoTelefono = new TelefonosCliente
                    {
                        ClienteId = usuarioDb.Cliente.Id,
                        TipoTelefono = "Principal",
                        NumeroTelefono = telefonoFormateado,
                        EsPrincipal = 1
                    };
                    await _context.TelefonosCliente.AddAsync(nuevoTelefono);
                }

                // actualiza direccion
                var direccionPrincipal = usuarioDb.Cliente.DireccionCliente
                    .FirstOrDefault(d => d.EsPrincipal == 1);

                if (direccionPrincipal != null)
                {
                    direccionPrincipal.DistritoId = distritoId!.Value;
                    direccionPrincipal.Detalles = detalles!;
                    _context.DireccionCliente.Update(direccionPrincipal);
                }
                else
                {
                    var nuevaDireccion = new DireccionCliente
                    {
                        ClienteId = usuarioDb.Cliente.Id,
                        DistritoId = distritoId!.Value,
                        TipoDireccion = "Principal",
                        Detalles = detalles!,
                        EsPrincipal = 1
                    };
                    await _context.DireccionCliente.AddAsync(nuevaDireccion);
                }
            }

            try
            {
                _context.Update(usuarioDb);
                await _context.SaveChangesAsync();
                await GuardarTelefonoUsuarioAsync(usuarioDb, telefonoFormateado);

                // La sesión identifica al usuario por su correo, así que hay que
                // rehacer la cookie: si no, al cambiarlo, las pantallas que lo
                // buscan por User.Identity.Name dejan de encontrarlo (404).
                await RefrescarSesionAsync(usuarioDb);

                await _bitacora.RegistrarAsync(
                    User,
                    "UPDATE",
                    "seg.USUARIO",
                    usuarioDb.Id,
                    $"Nombre: {usuarioDb.Nombre} {usuarioDb.ApellidoPaterno} {usuarioDb.ApellidoMaterno} | Correo: {usuarioDb.Correo} | Teléfono: {telefonoFormateado}",
                    valorAnterior);

                TempData["MensajeExito"] = "Información del perfil actualizada correctamente.";
                return RedirectToAction(nameof(Detalle));
            }
            catch
            {
                ModelState.AddModelError("", "Error al guardar los datos. Inténtalo de nuevo.");
                usuarioDb.Telefono = telefono;
                ViewBag.DireccionDetalles = detalles;
                await CargarListasDireccionAsync(usuarioDb, distritoId);
                return View(usuarioDb);
            }
        }

        //cambiar contrasena
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(
            int id,
            string contrasena,
            string confirmarContrasena)
        {
            var usuarioDb = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuarioDb == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                TempData["MensajeErrorContrasena"] = "La nueva contraseña es obligatoria.";
                return RedirectToAction(nameof(Editar));
            }

            if (contrasena != confirmarContrasena)
            {
                TempData["MensajeErrorContrasena"] = "Las contraseñas no coinciden.";
                return RedirectToAction(nameof(Editar));
            }

            usuarioDb.Contrasena = contrasena;

            try
            {
                _context.Update(usuarioDb);
                await _context.SaveChangesAsync();

                await _bitacora.RegistrarAsync(
                    User,
                    "UPDATE",
                    "seg.USUARIO",
                    usuarioDb.Id,
                    $"Cambio de contraseña del usuario {usuarioDb.Correo}");

                TempData["MensajeExito"] = "Contraseña actualizada correctamente.";
                return RedirectToAction(nameof(Detalle));
            }
            catch
            {
                TempData["MensajeErrorContrasena"] = "Ocurrió un error al cambiar la contraseña. Inténtalo de nuevo.";
                return RedirectToAction(nameof(Editar));
            }
        }

        private async Task AsegurarColumnaTelefonoUsuarioAsync()
        {
            const string sql = @"
IF COL_LENGTH('seg.USUARIO', 'telefono') IS NULL
    ALTER TABLE seg.USUARIO ADD telefono varchar(20) NULL;";
            await _context.Database.ExecuteSqlRawAsync(sql);
        }

        private async Task<string?> ObtenerTelefonoUsuarioAsync(Usuario usuario)
        {
            var telefonoCliente = usuario.Cliente?.TelefonosCliente?
                .FirstOrDefault(t => t.EsPrincipal == 1)?.NumeroTelefono;

            if (!string.IsNullOrWhiteSpace(telefonoCliente))
                return telefonoCliente.Trim();

            await AsegurarColumnaTelefonoUsuarioAsync();

            var telefonoUsuario = await _context.Database
                .SqlQueryRaw<string>("SELECT COALESCE(telefono, '') AS [Value] FROM seg.USUARIO WHERE id = @p0", usuario.Id)
                .FirstOrDefaultAsync();

            return string.IsNullOrWhiteSpace(telefonoUsuario) ? null : telefonoUsuario.Trim();
        }

        private async Task GuardarTelefonoUsuarioAsync(Usuario usuario, string? telefono)
        {
            await AsegurarColumnaTelefonoUsuarioAsync();

            object valorTelefono = string.IsNullOrWhiteSpace(telefono)
                ? DBNull.Value
                : telefono.Trim();

            await _context.Database.ExecuteSqlRawAsync(
                "UPDATE seg.USUARIO SET telefono = @p0 WHERE id = @p1",
                valorTelefono,
                usuario.Id);
        }

        // Si la cookie apunta a un correo que ya no existe (sesión vieja de antes
        // de cambiarlo), cerramos sesión en vez de dejar al usuario en un 404.
        private async Task<IActionResult> CerrarSesionPorCorreoInvalidoAsync()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("InicioSesion", "Auth");
        }

        // Vuelve a hacer la cookie de autenticación con los datos actuales del usuario.
        // Mismos claims que arma AuthController al iniciar sesión.
        private async Task RefrescarSesionAsync(Usuario usuario)
        {
            var declaraciones = new List<Claim>
            {
                new Claim(ClaimTypes.Name, usuario.Correo),
                new Claim("NombreCompleto", usuario.Nombre),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.RolId.ToString())
            };

            var identidad = new ClaimsIdentity(
                declaraciones,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identidad);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            HttpContext.User = principal;
        }

        // [AllowAnonymous]: estos dos endpoints solo devuelven el catálogo público
        // de cantones/distritos (sin datos de usuario), y los necesita también el
        // registro público de clientes (Auth/Registrar), que se llena sin sesión.
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetCantones(int provinciaId)
        {
            var cantones = await _context.Canton
                .Where(c => c.ProvinciaId == provinciaId)
                .OrderBy(c => c.Nombre)
                .Select(c => new { id = c.Id, nombre = c.Nombre })
                .ToListAsync();
            return Json(cantones);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetDistritos(int cantonId)
        {
            var distritos = await _context.Distrito
                .Where(d => d.CantonId == cantonId)
                .OrderBy(d => d.Nombre)
                .Select(d => new { id = d.Id, nombre = d.Nombre })
                .ToListAsync();
            return Json(distritos);
        }
    }
}