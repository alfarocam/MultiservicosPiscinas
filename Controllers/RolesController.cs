using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class RolesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Crear()
        {
            return View();
        }

        public IActionResult Asignar()
        {
            return View();
        }
    }
}
