using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            // Redirigir según rol:
            // 1 = Administrador → Dashboard
            // 2 = Técnico       → Dashboard
            // 3 = Cliente       → Portal
            var rolClaim = User.FindFirst(ClaimTypes.Role)?.Value;

            return rolClaim switch
            {
                "3" => RedirectToAction("Index", "Portal"),
                _ => RedirectToAction("Index", "Dashboard")
            };
        }
    }
}