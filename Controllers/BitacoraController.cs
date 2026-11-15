using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class BitacoraController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public BitacoraController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string accion, string orden, int? usuarioId, DateTime? desde, DateTime? hasta)
        {
            if (desde.HasValue && hasta.HasValue && desde.Value.Date > hasta.Value.Date)
            {
                ViewBag.MensajeError = "La fecha inicial no puede ser mayor que la fecha final.";
                desde = null;
                hasta = null;
            }

            ViewBag.FiltroAccion = accion;
            ViewBag.FiltroOrden = orden;
            ViewBag.FiltroUsuarioId = usuarioId;
            ViewBag.FiltroDesde = desde?.ToString("yyyy-MM-dd");
            ViewBag.FiltroHasta = hasta?.ToString("yyyy-MM-dd");

            // Solo usuarios que tienen registros en la bitácora.
            ViewBag.Usuarios = await _context.Usuario
                .Where(u => u.BitacoraAuditoria.Any())
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.ApellidoPaterno)
                .ToListAsync();

            var consulta = _context.BitacoraAuditoria.Include(b => b.Usuario).AsQueryable();

            if (!string.IsNullOrEmpty(accion))
            {
                consulta = consulta.Where(b => b.Accion == accion);
            }

            if (usuarioId.HasValue)
            {
                consulta = consulta.Where(b => b.UsuarioId == usuarioId.Value);
            }

            if (desde.HasValue)
            {
                consulta = consulta.Where(b => b.FechaHora >= desde.Value.Date);
            }

            if (hasta.HasValue)
            {
                // Se incluye todo el día final.
                consulta = consulta.Where(b => b.FechaHora < hasta.Value.Date.AddDays(1));
            }

            if (orden == "antiguos")
            {
                consulta = consulta.OrderBy(b => b.FechaHora);
            }
            else
            {

                consulta = consulta.OrderByDescending(b => b.FechaHora);
            }

            var listaBitacora = await consulta.ToListAsync();
            return View(listaBitacora);
        }

        // Detalle para ver el JSON de ValorAnterior y ValorNuevo (hacer la vista xd)
        public async Task<IActionResult> Detalle(int id)
        {
            var registro = await _context.BitacoraAuditoria
                .Include(b => b.Usuario)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (registro == null)
            {
                return NotFound();
            }

            return View(registro);
        }
    }
}