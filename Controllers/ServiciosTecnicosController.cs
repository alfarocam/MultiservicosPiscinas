using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1,2")]
    public class ServiciosTecnicosController(PiscinasYMultiserviciosContext context) : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context = context;

        public async Task<IActionResult> Index(string? busqueda)
        {
            var servicios = await _context.Servicio
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Cliente)
                            .ThenInclude(cl => cl.Usuario)
                .Where(s => s.Estado != "Cerrado")
                .OrderByDescending(s => s.FechaApertura)
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();
                servicios = servicios.Where(s =>
                    $"{s.Cita.Piscina.Cliente.Usuario.Nombre} {s.Cita.Piscina.Cliente.Usuario.ApellidoPaterno}"
                        .Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || s.Cita.Piscina.Tipo.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || s.Cita.Tipo.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || s.Estado.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || s.FechaApertura.ToString("dd/MM/yyyy").Contains(texto)
                ).ToList();
            }

            ViewBag.Busqueda = busqueda;
            return View(servicios);
        }

        public IActionResult Crear()
        {
            return View();
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var servicio = await _context.Servicio
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Cliente)
                            .ThenInclude(cl => cl.Usuario)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Direccion)
                            .ThenInclude(d => d.Distrito)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Tecnico)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servicio == null)
            {
                return NotFound();
            }

            return View(servicio);
        }
    }
}