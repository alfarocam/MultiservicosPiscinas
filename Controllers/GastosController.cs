using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    public class GastosController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;
        private readonly IWebHostEnvironment _env;

        public GastosController(PiscinasYMultiserviciosContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        
        public async Task<IActionResult> Index()
        {
            var gastos = await _context.GastoOperativo
                .Include(g => g.Categoria)
                .Include(g => g.Usuario)
                .OrderByDescending(g => g.Fecha)
                .ThenByDescending(g => g.Id)
                .ToListAsync();

            return View(gastos);
        }

        
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            ViewBag.Categorias = new SelectList(await _context.CategoriaGastoOperativo.ToListAsync(), "Id", "NombreCategoria");
            return View(new GastoOperativo { Fecha = DateOnly.FromDateTime(DateTime.Today) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(GastoOperativo gasto, IFormFile? comprobanteArchivo)
        {
            try
            {
                gasto.Estado = "Pendiente";
                if (gasto.Fecha == default)
                {
                    gasto.Fecha = DateOnly.FromDateTime(DateTime.Now);
                }

                var correo = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name;
                if (!string.IsNullOrEmpty(correo))
                {
                    var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);
                    if (usuario != null)
                    {
                        gasto.UsuarioId = usuario.Id;
                    }
                    else
                    {
                        var primerUsuario = await _context.Usuario.FirstOrDefaultAsync();
                        gasto.UsuarioId = primerUsuario?.Id ?? 1;
                    }
                }
                else
                {
                    var primerUsuario = await _context.Usuario.FirstOrDefaultAsync();
                    gasto.UsuarioId = primerUsuario?.Id ?? 1;
                }

                if (gasto.CitaId.HasValue && gasto.CitaId > 0)
                {
                    var existeCita = await _context.Cita.AnyAsync(c => c.Id == gasto.CitaId.Value);
                    if (!existeCita)
                    {
                        gasto.CitaId = null;
                    }
                }
                else
                {
                    gasto.CitaId = null;
                }

                if (comprobanteArchivo != null && comprobanteArchivo.Length > 0)
                {
                    gasto.ComprobanteRuta = await GuardarComprobanteAsync(comprobanteArchivo);
                }

                _context.GastoOperativo.Add(gasto);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "El gasto se registro correctamente y quedo en estadoo PENDIENTE de aprobación.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                string errorDetalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Error al guardar en base de datos: " + errorDetalle);
            }

            ViewBag.Categorias = new SelectList(await _context.CategoriaGastoOperativo.ToListAsync(), "Id", "NombreCategoria", gasto.CategoriaId);
            return View(gasto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var gasto = await _context.GastoOperativo.FindAsync(id);
            if (gasto != null)
            {
                gasto.Estado = "Aprobado";
                gasto.MotivoRechazo = null;
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "El gasto fue APROBADO exitosamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string motivoRechazo)
        {
            var gasto = await _context.GastoOperativo.FindAsync(id);
            if (gasto != null)
            {
                if (string.IsNullOrWhiteSpace(motivoRechazo))
                {
                    TempData["MensajeError"] = "Debe escribir una justificacipon para rechazar el gasto.";
                    return RedirectToAction(nameof(Index));
                }

                gasto.Estado = "Rechazado";
                gasto.MotivoRechazo = motivoRechazo;
                await _context.SaveChangesAsync();
                TempData["MensajeExito"] = "El gasto fue RECHAZADO.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Corregir(int id)
        {
            var gasto = await _context.GastoOperativo.FindAsync(id);
            if (gasto == null || gasto.Estado != "Rechazado")
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categorias = new SelectList(await _context.CategoriaGastoOperativo.ToListAsync(), "Id", "NombreCategoria", gasto.CategoriaId);
            return View(gasto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Corregir(GastoOperativo gastoForm, IFormFile? comprobanteArchivo)
        {
            try
            {
                var gastoExistente = await _context.GastoOperativo.FindAsync(gastoForm.Id);
                if (gastoExistente == null) return NotFound();

                gastoExistente.CategoriaId = gastoForm.CategoriaId;
                gastoExistente.Monto = gastoForm.Monto;
                gastoExistente.Descripcion = gastoForm.Descripcion;

                // Valido la citaid
                if (gastoForm.CitaId.HasValue && gastoForm.CitaId > 0)
                {
                    var existeCita = await _context.Cita.AnyAsync(c => c.Id == gastoForm.CitaId.Value);
                    gastoExistente.CitaId = existeCita ? gastoForm.CitaId : null;
                }
                else
                {
                    gastoExistente.CitaId = null;
                }

                
                gastoExistente.Estado = "Pendiente";
                gastoExistente.MotivoRechazo = null;

                if (comprobanteArchivo != null && comprobanteArchivo.Length > 0)
                {
                    gastoExistente.ComprobanteRuta = await GuardarComprobanteAsync(comprobanteArchivo);
                }

                _context.Update(gastoExistente);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "Corrección enviada correctamente. El gasto volvió a estado PENDIENTE.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                string errorDetalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Error al guardar la corrección: " + errorDetalle);
            }

            ViewBag.Categorias = new SelectList(await _context.CategoriaGastoOperativo.ToListAsync(), "Id", "NombreCategoria", gastoForm.CategoriaId);
            return View(gastoForm);
        }

        private async Task<string> GuardarComprobanteAsync(IFormFile archivo)
        {
            string carpetaSubidas = Path.Combine(_env.WebRootPath, "comprobantes");
            if (!Directory.Exists(carpetaSubidas)) Directory.CreateDirectory(carpetaSubidas);

            string nombreArchivoUnique = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName);
            string rutaFisica = Path.Combine(carpetaSubidas, nombreArchivoUnique);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return "/comprobantes/" + nombreArchivoUnique;
        }
    }
}