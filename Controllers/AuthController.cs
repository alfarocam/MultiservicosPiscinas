using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    public class AuthController : Controller
    {

        private readonly PiscinasYMultiserviciosContext _context;

        public AuthController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string correo, string contrasena)
        {
            //busca al usuario por correo en la base de datos
            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correo);

            //verifica si existe el usuario y si la contraseña coincide
            if (usuario != null && usuario.Contrasena == contrasena)
            {
                //inicia sesión creando una identidad con los datos del usuario
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.RolId.ToString()) //de aquí se saca el rol para que vean diferentes vistas
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                //autenticar al usuario en la aplicación
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
            ViewBag.Error = "Correo o contraseña incorrectos.";
            return View();
        }

        
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(Usuario usuario)
        {
            usuario.RolId = 3; //rol de cliente seteado por defecto en este caso
            usuario.FechaCreacion = DateTime.Now;

            try
            {
                //ejecutando el SP para pasar TODOS los parámetros(sin incluir el activo porque ese está declarado en el SP)
                var usuarioIdResult = await _context.Database
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

                var clienteResult = await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO cli.CLIENTE (usuario_id, notas) VALUES (@p0, @p1)",
                    nuevoUsuarioId,
                    (object)notasCliente ?? DBNull.Value
                );

                if (clienteResult <= 0)
                {
                    ViewBag.Mensaje= "Usuario creado, pero hubo un error al crear el perfil de cliente.";
                    return View();
                }

                return RedirectToAction("Login", "Auth");
            }
                catch(Exception ex)
            {
                ViewBag.Mensaje = "Ocurrió un error inesperado en el servidor.";
                return View();
            }
        }

        [HttpGet]
        public IActionResult RecoverPassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult RecoverPassword(string email)
        {
            // Implementation for password recovery
            return RedirectToAction("Login", "Auth");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Auth");
        }
    }
}
