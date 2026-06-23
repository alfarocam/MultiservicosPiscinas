using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class GastoOperativoController(
        PiscinasYMultiserviciosContext context,
        BitacoraService bitacora) : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context = context;
        private readonly BitacoraService _bitacora = bitacora;

        // GET: /GastoOperativo/Index
        public async Task<IActionResult> Index()
        {
            var gastos = await _context.GastoOperativo
                .Include(g => g.Categoria)
                .Include(g => g.Usuario)
                .OrderByDescending(g => g.Fecha)
                .ToListAsync();

            return View(gastos);
        }

        // GET: /GastoOperativo/Crear
        public async Task<IActionResult> Crear()
        {
            await CargarCategoriasAsync();
            return View(new GastoOperativoViewModel
            {
                Fecha = DateOnly.FromDateTime(DateTime.Today)
            });
        }

        // POST: /GastoOperativo/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(GastoOperativoViewModel model)
        {
            ModelState.Remove(nameof(GastoOperativoViewModel.Fecha));
            ModelState.Remove(nameof(GastoOperativoViewModel.Monto));

            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync();
                return View(model);
            }

            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("InicioSesion", "Auth");

            var usuario = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Correo == correo);

            if (usuario == null)
                return RedirectToAction("InicioSesion", "Auth");

            string? comprobanteRuta = null;
            if (model.Comprobante != null && model.Comprobante.Length > 0)
            {
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var extension = Path.GetExtension(model.Comprobante.FileName).ToLower();

                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError(nameof(model.Comprobante),
                        "Solo se permiten archivos JPG, PNG o PDF.");
                    await CargarCategoriasAsync();
                    return View(model);
                }

                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes");
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using var stream = new FileStream(rutaCompleta, FileMode.Create);
                await model.Comprobante.CopyToAsync(stream);

                comprobanteRuta = $"/comprobantes/{nombreArchivo}";
            }

            var gasto = new GastoOperativo
            {
                CategoriaId = model.CategoriaId,
                Monto = model.Monto,
                Fecha = model.Fecha,
                Descripcion = model.Descripcion,
                UsuarioId = usuario.Id,
                Estado = "Aprobado",
                ComprobanteRuta = comprobanteRuta
            };

            _context.GastoOperativo.Add(gasto);
            await _context.SaveChangesAsync();

            var categoriaNombre = (await _context.CategoriaGastoOperativo
                .FindAsync(model.CategoriaId))?.NombreCategoria ?? model.CategoriaId.ToString();

            await _bitacora.RegistrarAsync(
                userClaims: User,
                accion: "INSERT",
                tablaAfectada: "fin.GASTO_OPERATIVO",
                registroId: gasto.Id,
                valorNuevo: $"Categoría: {categoriaNombre} | Monto: ₡{model.Monto:N2} | Fecha: {model.Fecha:dd/MM/yyyy}"
            );

            TempData["MensajeExito"] = "Gasto registrado correctamente.";
            return RedirectToAction("Index", "GastoOperativo");
        }

        private async Task CargarCategoriasAsync()
        {
            ViewBag.Categorias = await _context.CategoriaGastoOperativo
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();
        }
    }
}
