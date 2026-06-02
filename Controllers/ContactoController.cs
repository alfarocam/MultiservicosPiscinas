using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    public class ContactoController : Controller
    {
        
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Enviar(ContactoViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            
            TempData["Exito"] = "La consulta fue enviada correctamente. Nos pondremos en contacto contigo pronto.";

            return RedirectToAction(nameof(Index));
        }

        
        public IActionResult Informacion()
        {
            return View();
        }
    }
}