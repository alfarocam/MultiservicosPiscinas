using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    public class AuthController(PiscinasYMultiserviciosContext _contexto, Generales _generales, IWebHostEnvironment _entornoWeb) : Controller
    {
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
            var usuario = await _contexto.Usuarios
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
                    return RedirectToAction("Index", "Portal");
                }

                return RedirectToAction("InicioSesion");
            }

            ViewBag.Mensaje = "Correo o contraseña incorrectos.";
            return View();
        }

        // =========================
        // REGISTRO
        // =========================

        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(Usuario usuario)
        {
            bool correoExiste = await _contexto.Usuarios
                .AnyAsync(u => u.Correo == usuario.Correo);

            if (correoExiste)
            {
                ViewBag.Mensaje = "El correo ya está registrado. Por favor, elige otro.";
                return View(usuario);
            }

            usuario.RolId = 3;
            usuario.FechaCreacion = DateTime.Now;

            try
            {
                var usuarioIdResult = await _contexto.Database
                    .SqlQueryRaw<int>(
                        "EXEC seg.InsertUserAndClient @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                        usuario.RolId,
                        usuario.Nombre,
                        usuario.ApellidoPaterno,
                        usuario.ApellidoMaterno,
                        usuario.Correo,
                        usuario.Contrasena,
                        usuario.FechaCreacion
                    )
                    .ToListAsync();

                int nuevoUsuarioId = usuarioIdResult.FirstOrDefault();

                if (nuevoUsuarioId <= 0)
                {
                    ViewBag.Mensaje = "Error al registrar el usuario.";
                    return View();
                }

                string notasCliente = "Cliente registrado desde registro de usuario.";

                var clienteResult = await _contexto.Database.ExecuteSqlRawAsync(
                    "INSERT INTO cli.CLIENTE (usuario_id, notas) VALUES (@p0, @p1)",
                    nuevoUsuarioId,
                    (object)notasCliente ?? DBNull.Value
                );

                if (clienteResult <= 0)
                {
                    ViewBag.Mensaje = "Usuario creado, pero hubo un error al crear el perfil de cliente.";
                    return View();
                }

                return RedirectToAction("InicioSesion", "Auth");
            }
            catch (Exception)
            {
                ViewBag.Mensaje = "Ocurrió un error inesperado en el servidor.";
                return View();
            }
        }

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