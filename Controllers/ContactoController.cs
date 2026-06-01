using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "3")]
    public class ContactoController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Enviar(ContactoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            TempData["Exito"] = "La consulta fue enviada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Informacion()
        {
            return View();
        }
    }
}