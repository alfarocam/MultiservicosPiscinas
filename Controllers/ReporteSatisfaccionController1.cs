using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    public class ReporteSatisfaccionController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public ReporteSatisfaccionController(
            PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var encuestas = await _context.Encuesta
                .Include(x => x.Servicio)
                .ToListAsync();

            ViewBag.TotalEncuestas =
                encuestas.Count;

            ViewBag.Promedio =
                encuestas.Any()
                ? encuestas.Average(x => x.Calificacion)
                : 0;

            ViewBag.Criticas =
                encuestas.Count(x => x.Calificacion <= 2);

            return View(encuestas);
        }
    }
}