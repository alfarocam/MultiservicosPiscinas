using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [HttpGet]
        [AllowAnonymous] //Permite que los no autenticados entren a esta acción
        public IActionResult Index()
        {
            if (User.Identity == null || !User.Identity.IsAuthenticated)
            {
                return RedirectToAction("InicioSesion", "Auth");
            }

            var roleClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            
            return roleClaim switch
            {
                "1" => RedirectToAction("Index", "Dashboard"),
                "2" => RedirectToAction("Index", "ServiciosTecnicos"),
                "3" => RedirectToAction("Index", "Portal"),
                _ => View() // El guion bajo (_) es el "default". Si no es ningún rol, muestra la vista común.
            };
        }

        // Destino de AccessDeniedPath: se muestra cuando un usuario autenticado
        // intenta abrir una vista que su rol no permite (devuelve HTTP 403).
        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }
    }
}