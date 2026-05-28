using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiservicosPiscinas.Models;
using System.Security.Claims;

namespace MultiservicosPiscinas.Controllers
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
        public IActionResult Login(string username)
        {
            return RedirectToAction("Index", "Home");
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

                usuario.Correo,
                usuario.Contrasena,
            if (result < 0)
            {
                //mensaje en caso de error
                ViewBag.Message = "Error al registrar el usuario.";
                    usuario.ApellidoMaterno,
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
        public IActionResult Logout()
        {
            // Simple redirect to login
            return RedirectToAction("Login", "Auth");
        }
    }
}
