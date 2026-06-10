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

            var consulta = new ConsultaContacto
            {
                Nombre = model.Nombre,
                Correo = model.Correo,
                Asunto = model.Asunto,
                Mensaje = model.Mensaje,
                FechaEnvio = DateTime.Now
            };

            _context.ConsultaContactos.Add(consulta);
            _context.SaveChanges();


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