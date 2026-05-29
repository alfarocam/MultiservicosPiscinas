using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    public class AuthController : Controller
    {

        private readonly PiscinasYMultiserviciosContext _contexto;
        private readonly Generales _generales;
        private readonly IWebHostEnvironment _entornoWeb;
        public AuthController(PiscinasYMultiserviciosContext context, Generales generales, IWebHostEnvironment entornoWeb)
        {
            _contexto = context;
            _generales = generales;
            _entornoWeb = entornoWeb;
        }

        [HttpGet]
        public IActionResult InicioSesion()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> InicioSesion(string correo, string contrasena)
        {
            //busca al usuario por correo en la base de datos
            var usuario = await _contexto.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo);

            //verifica si existe el usuario y si la contraseña coincide
            if (usuario != null && usuario.Contrasena == contrasena)
            {
                //inicia sesión creando una identidad con los datos del usuario
                var declaraciones = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.RolId.ToString()) //de aquí se saca el rol para que vean diferentes vistas
                };

                var identidadDeclaraciones = new ClaimsIdentity(declaraciones, CookieAuthenticationDefaults.AuthenticationScheme);

                //autenticar al usuario en la aplicación
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identidadDeclaraciones));

                return RedirectToAction("Index", "Home");
            }
            ViewBag.Mensaje = "Correo o contraseña incorrectos.";
            return View();
        }


        [HttpGet]
        public IActionResult Registrar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registrar(Usuario usuario)
        {
            //se usa AnyAsync y no FirstOrDefaultAsync porque con Any se deja de 
            //buscar apenas se encuentra una coincidencia, ahorrando recurso y ayudando el tiempo de procesamiento
            bool correoExiste = await _contexto.Usuarios
            .AnyAsync(u => u.Correo == usuario.Correo);

            if (correoExiste)
            {
                ViewBag.Mensaje = "El correo ya está registrado. Por favor, elige otro.";
                return View(usuario);
            }

            usuario.RolId = 3; //rol de cliente seteado por defecto en este caso
            usuario.FechaCreacion = DateTime.Now;

            try
            {
                //ejecutando el SP para pasar TODOS los parámetros(sin incluir el activo porque ese está declarado en el SP)
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
                    //mensaje en caso de error
                    ViewBag.Message = "Error al registrar el usuario.";
                    return View();
                }
                //luego de insertar en la tabla usuario, se inserta en cliente
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
                .SqlQuery<ResultadoValidacionUsuario>($"EXEC seg.ValidarCorreoRecuperacion @Correo={correoLimpio}")
                .AsAsyncEnumerable()
                .FirstOrDefaultAsync();

            if (resultado == null)
            {
                ViewBag.Mensaje = "Su información no se validó correctamente.";
                return View();
            }

            var nuevaContrasena = _generales.GenerarContrasena();

            //actualizamos la contraseña
            int filasAfectadas = await _contexto.Database
                .ExecuteSqlAsync($"EXEC seg.ActualizarContrasena @Contrasena={nuevaContrasena}, @IdUsuario={resultado.Id}");

            if (filasAfectadas <= 0)
            {
                ViewBag.Mensaje = "Su información no se actualizó correctamente.";
                return View();
            }

            //se accede a la carpeta templates para enviar el correo con la nueva contraseña
            string rutaHtml = Path.Combine(_entornoWeb.ContentRootPath, "Template", "RecuperarContrasena.html");

            if (!System.IO.File.Exists(rutaHtml))
            {
                ViewBag.Mensaje = "Error interno: No se encontró la plantilla de correo.";
                return View();
            }

            string contenidoHtml = await System.IO.File.ReadAllTextAsync(rutaHtml);

            string htmlFinal = contenidoHtml
                .Replace("{{NOMBRE_USUARIO}}", resultado.Nombre)
                .Replace("{{NUEVA_CONTRASENA}}", nuevaContrasena);

            //envío del correo electrónico
            _generales.EnviarCorreo(resultado.Correo, "Recuperar Acceso", htmlFinal);

            return RedirectToAction("InicioSesion", "Auth");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("InicioSesion", "Auth");
        }

        //DTO
        public class ResultadoValidacionUsuario
        {
            public int Id { get; set; }
            public string Nombre { get; set; } = string.Empty;
            public string Correo { get; set; } = string.Empty;
        }
    }
}
