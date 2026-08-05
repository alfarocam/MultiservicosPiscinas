using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    public class AuthController(PiscinasYMultiserviciosContext _contexto, Generales _generales, IWebHostEnvironment _entornoWeb, IConfiguration _configuration) : Controller
    {
        #region Iniciar Sesión
        // =========================
        // LOGIN
        // =========================

        [HttpGet]
        public IActionResult InicioSesion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InicioSesion(string correo, string contrasena)
        {
            // Buscar usuario por correo
            var usuario = await _contexto.Usuario
                .FirstOrDefaultAsync(u => u.Correo == correo);

            // Validar existencia y contraseña
            if (usuario != null &&
                usuario.Contrasena == contrasena &&
                usuario.Activo == true)
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

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identidad));

                // REDIRECCIÓN SEGÚN ROL
                if (usuario.RolId == 1) // Admin
                {
                    return RedirectToAction("Index", "Dashboard");
                }
                else if (usuario.RolId == 2) // Técnico
                {
                    return RedirectToAction("Index", "ServiciosTecnicos");
                }
                else if (usuario.RolId == 3) // Cliente
                {
                    return RedirectToAction("Index", "Tienda");
                }

                return RedirectToAction("InicioSesion");
            }

            ViewBag.Mensaje = "Correo o contraseña incorrectos.";
            return View();
        }
        #endregion

        #region Registro
        // =========================
        // REGISTRO
        // =========================

        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            await CargarDatosUbicacionAsync();
            return View();
        }

        // Provincias + API key de Google Maps para el selector de ubicación del
        // registro público. Mismo patrón que ClientesController.CargarProvinciasAsync,
        // reutilizado acá porque este formulario ahora pide la misma dirección.
        private async Task CargarDatosUbicacionAsync()
        {
            ViewBag.Provincias = await _contexto.Provincia.OrderBy(p => p.Nombre).ToListAsync();
            ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"] ?? string.Empty;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(UsuarioRegistroViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosUbicacionAsync();
                return View(model);
            }

            bool correoExiste = await _contexto.Usuario
                .AnyAsync(u => u.Correo == model.Correo);

            if (correoExiste)
            {
                ViewBag.Mensaje = "El correo ya está registrado. Por favor, elige otro.";
                ViewBag.TipoMensaje = "danger";
                await CargarDatosUbicacionAsync();
                return View(model);
            }

            const int rolCliente = 3;
            var fechaCreacion = DateTime.Now;

            try
            {
                var usuarioIdResult = await _contexto.Database
                    .SqlQueryRaw<int>(
                        "EXEC seg.InsertarUsuarioYCliente @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                        rolCliente,
                        model.Nombre,
                        model.ApellidoPaterno,
                        model.ApellidoMaterno,
                        model.Correo,
                        model.Contrasena,
                        fechaCreacion
                    )
                    .ToListAsync();

                int nuevoUsuarioId = usuarioIdResult.FirstOrDefault();

                if (nuevoUsuarioId <= 0)
                {
                    ViewBag.Mensaje = "Error al registrar el usuario.";
                    ViewBag.TipoMensaje = "danger";
                    await CargarDatosUbicacionAsync();
                    return View(model);
                }

                // A partir de acá se usa EF directamente (en vez de más INSERT en
                // SQL crudo) porque se necesita el Id autogenerado del cliente
                // para poder guardar su teléfono y dirección a continuación.
                var cliente = new Cliente
                {
                    UsuarioId = nuevoUsuarioId,
                    Notas = "Cliente registrado desde registro de usuario."
                };
                _contexto.Cliente.Add(cliente);
                await _contexto.SaveChangesAsync();

                _contexto.TelefonosCliente.Add(new TelefonosCliente
                {
                    ClienteId = cliente.Id,
                    TipoTelefono = "Principal",
                    NumeroTelefono = model.Telefono,
                    EsPrincipal = 1
                });

                _contexto.DireccionCliente.Add(new DireccionCliente
                {
                    ClienteId = cliente.Id,
                    DistritoId = model.DistritoId,
                    TipoDireccion = "Principal",
                    Detalles = model.Direccion,
                    EsPrincipal = 1,
                    Latitud = model.Latitud,
                    Longitud = model.Longitud
                });

                await _contexto.SaveChangesAsync();

                ViewBag.Mensaje = "Usuario creado correctamente.";
                ViewBag.TipoMensaje = "success";
                await CargarDatosUbicacionAsync();
                return View();
            }
            catch (Exception)
            {
                ViewBag.Mensaje = "Ocurrió un error inesperado en el servidor.";
                ViewBag.TipoMensaje = "danger";
                await CargarDatosUbicacionAsync();
                return View(model);
            }
        }
        #endregion

        #region Recuperar Contraseña
        // =========================
        // RECUPERAR CONTRASEÑA
        // =========================

        [HttpGet]
        public IActionResult RecuperarContrasena()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> RecuperarContrasena(Usuario usuario)
        {
            var correoLimpio = usuario.Correo.Trim();

            var resultado = await _contexto.Database
                .SqlQuery<ResultadoValidacionUsuario>(
                    $"EXEC seg.ValidarCorreoRecuperacion @Correo={correoLimpio}")
                .AsAsyncEnumerable()
                .FirstOrDefaultAsync();

            if (resultado == null)
            {
                ViewBag.Mensaje = "Su información no se validó correctamente.";
                return View();
            }

            var nuevaContrasena = _generales.GenerarContrasena();

            int filasAfectadas = await _contexto.Database
                .ExecuteSqlAsync(
                    $"EXEC seg.ActualizarContrasena @Contrasena={nuevaContrasena}, @IdUsuario={resultado.Id}");

            if (filasAfectadas <= 0)
            {
                ViewBag.Mensaje = "Su información no se actualizó correctamente.";
                return View();
            }

            string rutaHtml = Path.Combine(
                _entornoWeb.ContentRootPath,
                "Template",
                "RecuperarContrasena.html");

            if (!System.IO.File.Exists(rutaHtml))
            {
                ViewBag.Mensaje = "Error interno: No se encontró la plantilla de correo.";
                return View();
            }

            string contenidoHtml = await System.IO.File.ReadAllTextAsync(rutaHtml);

            string htmlFinal = contenidoHtml
                .Replace("{{NOMBRE_USUARIO}}", resultado.Nombre)
                .Replace("{{NUEVA_CONTRASENA}}", nuevaContrasena);

            _generales.EnviarCorreo(
                resultado.Correo,
                "Recuperar Acceso",
                htmlFinal);

            return RedirectToAction("InicioSesion", "Auth");
        }
        #endregion

        #region Cerrar Sesion
        // =========================
        // CERRAR SESIÓN
        // =========================

        [HttpPost]
        public async Task<IActionResult> CerrarSesion()
        {
            await HttpContext.SignOutAsync(
                CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("InicioSesion", "Auth");
        }
        #endregion

        // =========================
        // DTO
        // =========================

        public class ResultadoValidacionUsuario
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Correo { get; set; } = string.Empty;
        }
    }
}