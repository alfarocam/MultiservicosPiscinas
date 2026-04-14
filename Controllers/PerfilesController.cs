using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiservicosPiscinas.Controllers
{
    [Authorize]
    public class PerfilesController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Detalle()
        {
            return View();
        }

        public IActionResult Editar()
        {
            return View();
        }
    }
}