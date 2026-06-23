using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class GastosController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public GastosController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> PanelTecnico()
        {

            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int tecnicoId)) return RedirectToAction("Login", "Account");

            var historial = await _context.GastoOperativo
                .Where(g => g.UsuarioId == tecnicoId)
                .OrderByDescending(g => g.Fecha)
                .ToListAsync();

            return View(historial);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegistrarGasto(decimal monto, string categoria, string descripcion, int? gastoId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdString, out int tecnicoId)) return Unauthorized();

            if (monto <= 0 || string.IsNullOrEmpty(categoria) || string.IsNullOrEmpty(descripcion))
            {
                TempData["Error"] = "Todos los campos son obligatorios y el monto debe ser mayor a 0.";
                return RedirectToAction(nameof(PanelTecnico));
            }

            if (gastoId.HasValue && gastoId.Value > 0)
            {

                var gastoExistente = await _context.GastoOperativo.FindAsync(gastoId.Value);
                if (gastoExistente != null && gastoExistente.UsuarioId == tecnicoId)
                {
                    gastoExistente.Monto = monto;
                    gastoExistente.CategoriaId = categoria == "Viáticos" ? 1 : 2;
                    gastoExistente.Descripcion = descripcion;
                    gastoExistente.Estado = "Pendiente";
                    gastoExistente.MotivoRechazo = null;
                    _context.GastoOperativo.Update(gastoExistente);
                    TempData["Exito"] = "Gasto corregido y enviado a aprobación nuevamente.";
                }
            }
            else
            {

                var nuevoGasto = new GastoOperativo
                {
                    UsuarioId = tecnicoId,
                    CategoriaId = categoria == "Viáticos" ? 1 : 2,
                    Monto = monto,
                    Fecha = DateOnly.FromDateTime(DateTime.Today),
                    Descripcion = descripcion,
                    Estado = "Pendiente"
                };
                _context.GastoOperativo.Add(nuevoGasto);
                TempData["Exito"] = "Gasto registrado con éxito y asociado a su cuenta técnica.";
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(PanelTecnico));
        }

        [Authorize(Roles = "1")] 
        public async Task<IActionResult> PanelAdmin()
        {

            var pendientes = await _context.GastoOperativo
                .Include(g => g.Usuario)
                .Where(g => g.Estado == "Pendiente")
                .ToListAsync();

            return View(pendientes);
        }


        [HttpPost]
        [Authorize(Roles = "1")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcesarGasto(int id, string accion, string motivoRechazo)
        {
            var gasto = await _context.GastoOperativo.FindAsync(id);
            if (gasto == null) return NotFound();

            if (accion == "Aprobar")
            {
                gasto.Estado = "Aprobado";
                gasto.MotivoRechazo = null;
            }
            else if (accion == "Rechazar")
            {
                if (string.IsNullOrEmpty(motivoRechazo))
                {
                    TempData["ErrorAdmin"] = "Debe especificar una justificación para rechazar el gasto.";
                    return RedirectToAction(nameof(PanelAdmin));
                }
                gasto.Estado = "Rechazado";
                gasto.MotivoRechazo = motivoRechazo;
            }

            _context.GastoOperativo.Update(gasto);
            await _context.SaveChangesAsync();

            TempData["ExitoAdmin"] = $"Gasto #{id} actualizado a '{gasto.Estado}' correctamente.";
            return RedirectToAction(nameof(PanelAdmin));
        }
    }
}