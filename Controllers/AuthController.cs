using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiservicosPiscinas.Models;

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
            usuario.RolId = 3; //rol de cliente por defecto
            usuario.FechaCreacion = DateTime.Now;

            var result = await _context.Database.ExecuteSqlRawAsync(
                "EXEC seg.InsertUser @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                usuario.RolId,
                usuario.Nombre,
                usuario.ApellidoPaterno,
                usuario.ApellidoMaterno,
                usuario.Correo,
                usuario.Contrasena,
                usuario.FechaCreacion
            );
            if (result < 0)
            {
                //mensaje en caso de error
                ViewBag.Message = "Error al registrar el usuario.";
                return View();
            }

            return RedirectToAction("Login", "Auth");
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
