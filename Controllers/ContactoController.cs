using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    public class ContactoController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public ContactoController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

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

            TempData["Exito"] =
                "La consulta fue enviada correctamente.";

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Informacion()
        {
            var contacto =
                _context.ContactoEmpresas.FirstOrDefault();

            return View(contacto);
        }
    }
}