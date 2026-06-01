using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    public class EncuestasController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public EncuestasController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public IActionResult Crear(int servicioId)
        {
            var encuesta = new Encuestum
            {
                ServicioId = servicioId
            };

            return View(encuesta);
        }

        [HttpPost]
        public async Task<IActionResult> Crear(Encuestum encuesta)
        {
            bool existe = await _context.Encuesta
                .AnyAsync(x => x.ServicioId == encuesta.ServicioId);

            if (existe)
            {
                TempData["Error"] =
                    "Este servicio ya posee una encuesta.";

                return RedirectToAction("Index", "Portal");
            }

            encuesta.FechaEnvio = DateTime.Now;

            _context.Encuesta.Add(encuesta);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "Encuesta registrada correctamente.";

            return RedirectToAction("Index", "Portal");
        }
    }
}