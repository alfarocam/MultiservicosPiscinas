using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiservicosPiscinas.Data;
using MultiservicosPiscinas.Models;

namespace MultiservicosPiscinas.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class PiscinasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PiscinasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Piscinas
        public async Task<IActionResult> Index()
        {
            var piscinas = await _context.Piscinas
                .Include(p => p.Cliente)
                .Where(p => p.Activa)
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            return View(piscinas);
        }

        // GET: Piscinas/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var piscina = await _context.Piscinas
                .Include(p => p.Cliente)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (piscina == null)
                return NotFound();

            return View(piscina);
        }

        // GET: Piscinas/Crear
        public async Task<IActionResult> Crear()
        {
            await CargarClientesActivos();
            return View();
        }

        // POST: Piscinas/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Piscina piscina)
        {
            if (ModelState.IsValid)
            {
                _context.Piscinas.Add(piscina);
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Piscina creada correctamente.";
                return RedirectToAction(nameof(Index));
            }

            await CargarClientesActivos(piscina.ClienteId);
            return View(piscina);
        }

        // GET: Piscinas/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var piscina = await _context.Piscinas.FindAsync(id);

            if (piscina == null)
                return NotFound();

            await CargarClientesActivos(piscina.ClienteId);
            return View(piscina);
        }

        // POST: Piscinas/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Piscina piscina)
        {
            if (id != piscina.Id)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(piscina);
                    await _context.SaveChangesAsync();
                    TempData["Exito"] = "Piscina actualizada correctamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateException)
                {
                    ModelState.AddModelError("", "Ocurrió un error al actualizar la piscina.");
                }
            }

            await CargarClientesActivos(piscina.ClienteId);
            return View(piscina);
        }

        // POST: Piscinas/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var piscina = await _context.Piscinas.FindAsync(id);

            if (piscina != null)
            {
                piscina.Activa = false;
                await _context.SaveChangesAsync();
                TempData["Exito"] = "Piscina eliminada correctamente.";
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarClientesActivos(object? seleccionado = null)
        {
            var clientes = await _context.Clientes
                .Where(c => c.Activo)
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.ClienteId = new SelectList(clientes, "Id", "Nombre", seleccionado);
        }
    }
}