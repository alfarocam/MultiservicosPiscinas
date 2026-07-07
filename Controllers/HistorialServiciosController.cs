using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    public class HistorialServiciosController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public HistorialServiciosController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // simulamos con el ID 1 de la tabla Cliente Para el sprint xd
            int clienteIdLogueado = 1;

            var historialServicios = await _context.Servicio
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Tecnico)
                .Where(s => s.Cita.Piscina.ClienteId == clienteIdLogueado)
                .OrderByDescending(s => s.FechaApertura) 
                .ToListAsync();

            return View(historialServicios);
        }
    }
}