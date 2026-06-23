using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class BitacoraController(PiscinasYMultiserviciosContext context) : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context = context;

        // GET: /Bitacora/Index
        public async Task<IActionResult> Index()
        {
            var registros = await _context.BitacoraAuditoria
                .Include(b => b.Usuario)
                .OrderByDescending(b => b.FechaHora)
                .ToListAsync();

            return View(registros);
        }

        // GET: /Bitacora/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var registro = await _context.BitacoraAuditoria
                .Include(b => b.Usuario)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (registro == null)
                return NotFound();

            return View(registro);
        }
    }
}