using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "3")]
    public class EncuestasController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public EncuestasController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Crear(int servicioId)
        {
            bool existe = await _context.Encuesta
                .AnyAsync(x => x.ServicioId == servicioId);

            if (existe)
            {
                TempData["Error"] =
                    "Este servicio ya ha sido calificado, gracias.";

                return RedirectToAction(
                    "Detalle",
                    "Historial",
                    new { id = servicioId });
            }

            var encuesta = new Encuestum
            {
                ServicioId = servicioId
            };

            return View(encuesta);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Encuestum encuesta)
        {
            if (encuesta.Calificacion < 1 || encuesta.Calificacion > 5)
            {
                ModelState.AddModelError(
                    "Calificacion",
                    "La calificación es obligatoria.");

                return View(encuesta);
            }

            bool existe = await _context.Encuesta
                .AnyAsync(x => x.ServicioId == encuesta.ServicioId);

            if (existe)
            {
                TempData["Error"] =
                    "Este servicio ya ha sido calificado, gracias.";

                return RedirectToAction(
                    "Detalle",
                    "Historial",
                    new { id = encuesta.ServicioId });
            }

            encuesta.FechaEnvio = DateTime.Now;

            _context.Encuesta.Add(encuesta);

            await _context.SaveChangesAsync();

            TempData["Exito"] =
                "La calificación fue enviada exitosamente.";

            return RedirectToAction(
                "Detalle",
                "Historial",
                new { id = encuesta.ServicioId });
        }
    }
}