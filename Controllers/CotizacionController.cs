using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class CotizacionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Crear()
        {
            return View();
        }

        public IActionResult VistaPrevia()
        {
            return View();
        }
    }
}
