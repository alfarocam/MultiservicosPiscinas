using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    public class BitacoraController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public BitacoraController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string accion, string tabla, string orden)
        {
            ViewBag.FiltroAccion = accion;
            ViewBag.FiltroTabla = tabla;
            ViewBag.FiltroOrden = orden;

            var consulta = _context.BitacoraAuditoria.Include(b => b.Usuario).AsQueryable();

            if (!string.IsNullOrEmpty(accion))
            {
                consulta = consulta.Where(b => b.Accion == accion);
            }

            if (!string.IsNullOrEmpty(tabla))
            {
                consulta = consulta.Where(b => b.TablaAfectada.Contains(tabla));
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