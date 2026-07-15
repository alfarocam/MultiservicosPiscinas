using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    public class CambiarTecnicoDaniController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public CambiarTecnicoDaniController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        //muestroo los servicios con el tec asignado
        public async Task<IActionResult> Index()
        {
            var servicios = await _context.Servicio
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Cliente)
                            .ThenInclude(cli => cli.Usuario)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Tecnico)
                .OrderByDescending(s => s.FechaApertura)
                .ToListAsync();

            var tecnicos = await _context.Usuario
                .Select(u => new {
                    u.Id,
                    NombreCompleto = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno
                })
                .ToListAsync();

            ViewBag.Tecnicos = new SelectList(tecnicos, "Id", "NombreCompleto");

            return View(servicios);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Asignar(int servicioId, int nuevoTecnicoId)
        {
            var servicio = await _context.Servicio
                .Include(s => s.Cita)
                .FirstOrDefaultAsync(s => s.Id == servicioId);

            if (servicio != null && servicio.Cita != null)
            {
                servicio.Cita.TecnicoId = nuevoTecnicoId;

                _context.Update(servicio.Cita);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "Técnico reasignado prueba daniiiip!";
            }
            else
            {
                TempData["MensajeError"] = "No se pudo reasignar el técnico Intente de nuevo.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}