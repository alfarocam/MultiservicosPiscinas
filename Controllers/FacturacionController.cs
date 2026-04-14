using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MultiservicosPiscinas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class FacturacionController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Crear()
        {
            return View();
        }

        public IActionResult Detalle(int id)
        {
            return View();
        }
    }
}