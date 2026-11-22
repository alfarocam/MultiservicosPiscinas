using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Data;
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
                .OrderBy(g => g.Estado == "Pendiente" ? 1 : (g.Estado == "Aprobado" ? 2 : 3))
                .ThenByDescending(g => g.Fecha)
                .ThenByDescending(g => g.Id)
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

            // Sin comprobante el gasto se rechaza automáticamente.
            bool sinComprobante = string.IsNullOrEmpty(comprobanteRuta);
            if (sinComprobante)
            {
                gasto.Estado = "Rechazado";
                gasto.MotivoRechazo = "Rechazado automáticamente: el gasto no tiene comprobante adjunto.";
            }

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

            if (sinComprobante)
                TempData["MensajeError"] = "El gasto fue registrado pero quedó RECHAZADO automáticamente por no tener comprobante.";
            else
                TempData["MensajeExito"] = "Gasto registrado correctamente.";
            return RedirectToAction("Index", "GastoOperativo");
        }

        // GET: /GastoOperativo/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var gasto = await _context.GastoOperativo
                .Include(g => g.Categoria)
                .Include(g => g.Usuario)
                .Include(g => g.Cita).ThenInclude(c => c!.Piscina)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gasto == null) return NotFound();

            ViewBag.VolverController = "GastoOperativo";
            return View("~/Views/Gastos/Detalle.cshtml", gasto);
        }

        // POST: /GastoOperativo/Aprobar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aprobar(int id)
        {
            var gasto = await _context.GastoOperativo.FindAsync(id);
            if (gasto != null)
            {
                string estadoAnterior = gasto.Estado;

                if (string.IsNullOrEmpty(gasto.ComprobanteRuta))
                {
                    gasto.Estado = "Rechazado";
                    gasto.MotivoRechazo = "Rechazado automáticamente: el gasto no tiene comprobante adjunto.";
                    await _context.SaveChangesAsync();
                    await _bitacora.RegistrarAsync(
                        userClaims: User,
                        accion: "UPDATE",
                        tablaAfectada: "fin.GASTO_OPERATIVO",
                        registroId: gasto.Id,
                        valorAnterior: $"Estado: {estadoAnterior}",
                        valorNuevo: $"Estado: {gasto.Estado} | Motivo: {gasto.MotivoRechazo}"
                    );
                    TempData["MensajeError"] = "El gasto no tiene comprobante, por lo que fue RECHAZADO automáticamente.";
                    return RedirectToAction(nameof(Index));
                }

                gasto.Estado = "Aprobado";
                gasto.MotivoRechazo = null;
                await _context.SaveChangesAsync();
                await _bitacora.RegistrarAsync(
                    userClaims: User,
                    accion: "UPDATE",
                    tablaAfectada: "fin.GASTO_OPERATIVO",
                    registroId: gasto.Id,
                    valorAnterior: $"Estado: {estadoAnterior}",
                    valorNuevo: $"Estado: {gasto.Estado} | Monto: ₡{gasto.Monto:N2}"
                );
                TempData["MensajeExito"] = "El gasto fue APROBADO exitosamente.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /GastoOperativo/Rechazar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Rechazar(int id, string motivoRechazo)
        {
            var gasto = await _context.GastoOperativo.FindAsync(id);
            if (gasto != null)
            {
                if (string.IsNullOrWhiteSpace(motivoRechazo))
                {
                    TempData["MensajeError"] = "Debe escribir una justificación para rechazar el gasto.";
                    return RedirectToAction(nameof(Index));
                }

                string estadoAnterior = gasto.Estado;
                string motivoLimpio = motivoRechazo.Trim();

                gasto.Estado = "Rechazado";
                gasto.MotivoRechazo = motivoLimpio;
                await _context.SaveChangesAsync();
                await _bitacora.RegistrarAsync(
                    userClaims: User,
                    accion: "UPDATE",
                    tablaAfectada: "fin.GASTO_OPERATIVO",
                    registroId: gasto.Id,
                    valorAnterior: $"Estado: {estadoAnterior}",
                    valorNuevo: $"Estado: {gasto.Estado} | Motivo: {motivoLimpio}"
                );
                TempData["MensajeExito"] = "El gasto fue RECHAZADO.";
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /GastoOperativo/CrearCategoria
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearCategoria(string nombreCategoria, string? returnUrl = null)
        {
            var (exito, mensaje, nuevaCategoria) =
                await DivisionTerritorialSeeder.RegistrarCategoriaGastoAsync(_context, nombreCategoria);

            if (!exito || nuevaCategoria == null)
            {
                TempData["MensajeError"] = mensaje;
                return RedirigirRetorno(returnUrl);
            }

            await _bitacora.RegistrarAsync(
                userClaims: User,
                accion: "INSERT",
                tablaAfectada: "fin.CATEGORIA_GASTO_OPERATIVO",
                registroId: nuevaCategoria.Id,
                valorNuevo: $"Categoría: {nuevaCategoria.NombreCategoria}"
            );

            TempData["MensajeExito"] = mensaje;
            return RedirigirRetorno(returnUrl);
        }

        private IActionResult RedirigirRetorno(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        private async Task AsegurarCategoriasBaseAsync()
        {
            await DivisionTerritorialSeeder.AsegurarCategoriasGastoOperativoAsync(_context);
        }

        private async Task CargarCategoriasAsync()
        {
            await AsegurarCategoriasBaseAsync();
            ViewBag.Categorias = await _context.CategoriaGastoOperativo
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();
        }
    }
}
