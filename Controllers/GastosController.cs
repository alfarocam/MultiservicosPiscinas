using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Data;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1,2")]
    public class GastosController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly BitacoraService _bitacora;

        private const string MotivoSinComprobante = "Rechazado automáticamente: el gasto no tiene comprobante adjunto.";

        public GastosController(
            PiscinasYMultiserviciosContext context,
            IWebHostEnvironment env,
            BitacoraService bitacora)
        {
            _context = context;
            _env = env;
            _bitacora = bitacora;
        }

        
        public async Task<IActionResult> Index()
        {
            var query = _context.GastoOperativo
                .Include(g => g.Categoria)
                .Include(g => g.Usuario)
                .AsQueryable();

            if (!User.IsInRole("1"))
            {
                var usuarioId = await ObtenerUsuarioActualIdAsync();
                if (usuarioId == null)
                    return RedirectToAction("InicioSesion", "Auth");

                query = query.Where(g => g.UsuarioId == usuarioId.Value);
            }

            var gastos = await query
                .OrderByDescending(g => g.Fecha)
                .ThenByDescending(g => g.Id)
                .ToListAsync();

            ViewBag.Categorias = await ObtenerCategoriasAsync();
            return View(gastos);
        }

        
        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            ViewBag.Categorias = new SelectList(await ObtenerCategoriasAsync(), "Id", "NombreCategoria");
            return View(new GastoOperativo { Fecha = DateOnly.FromDateTime(DateTime.Today) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(GastoOperativo gasto, IFormFile? comprobanteArchivo)
        {
            ModelState.Remove(nameof(GastoOperativo.Estado));
            ModelState.Remove(nameof(GastoOperativo.Categoria));
            ModelState.Remove(nameof(GastoOperativo.Usuario));
            ModelState.Remove(nameof(GastoOperativo.Fecha));
            ModelState.Remove(nameof(GastoOperativo.Monto));
            ModelState.Remove(nameof(GastoOperativo.CategoriaId));

            gasto.Descripcion = gasto.Descripcion?.Trim();

            if (gasto.CategoriaId <= 0 || !await _context.CategoriaGastoOperativo.AnyAsync(c => c.Id == gasto.CategoriaId))
            {
                ModelState.AddModelError(nameof(GastoOperativo.CategoriaId), "Debe seleccionar una categoría válida.");
            }

            if (gasto.Monto <= 0 || gasto.Monto > 999999999.99m)
            {
                ModelState.AddModelError(nameof(GastoOperativo.Monto), "El monto debe ser mayor a cero.");
            }

            if (gasto.Fecha == default || gasto.Fecha.Year < 1753)
            {
                ModelState.AddModelError(nameof(GastoOperativo.Fecha), "Debe seleccionar una fecha válida.");
            }

            if (!string.IsNullOrEmpty(gasto.Descripcion) && gasto.Descripcion.Length > 500)
            {
                ModelState.AddModelError(nameof(GastoOperativo.Descripcion), "La descripción no puede superar los 500 caracteres.");
            }

            if (comprobanteArchivo != null && comprobanteArchivo.Length > 0)
            {
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var extension = Path.GetExtension(comprobanteArchivo.FileName).ToLowerInvariant();
                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError("comprobanteArchivo", "Solo se permiten archivos JPG, PNG o PDF.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Categorias = new SelectList(await ObtenerCategoriasAsync(), "Id", "NombreCategoria", gasto.CategoriaId);
                return View(gasto);
            }

            try
            {
                var usuarioId = await ObtenerUsuarioActualIdAsync();
                if (usuarioId == null)
                {
                    return RedirectToAction("InicioSesion", "Auth");
                }

                gasto.UsuarioId = usuarioId.Value;

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

                // Sin comprobante el gasto se rechaza automáticamente.
                bool sinComprobante = string.IsNullOrEmpty(gasto.ComprobanteRuta);
                bool esAdmin = User.IsInRole("1");

                if (sinComprobante)
                {
                    gasto.Estado = "Rechazado";
                    gasto.MotivoRechazo = MotivoSinComprobante;
                }
                else if (esAdmin)
                {
                    gasto.Estado = "Aprobado";
                    gasto.MotivoRechazo = null;
                }
                else
                {
                    gasto.Estado = "Pendiente";
                    gasto.MotivoRechazo = null;
                }

                _context.GastoOperativo.Add(gasto);
                await _context.SaveChangesAsync();

                var categoriaNombre = (await _context.CategoriaGastoOperativo
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.Id == gasto.CategoriaId))?.NombreCategoria ?? gasto.CategoriaId.ToString();

                await _bitacora.RegistrarAsync(
                    userClaims: User,
                    accion: "INSERT",
                    tablaAfectada: "fin.GASTO_OPERATIVO",
                    registroId: gasto.Id,
                    valorNuevo: $"Categoría: {categoriaNombre} | Monto: ₡{gasto.Monto:N2} | Fecha: {gasto.Fecha:dd/MM/yyyy} | Estado: {gasto.Estado}"
                );

                if (sinComprobante)
                {
                    TempData["MensajeError"] = esAdmin
                        ? "El gasto fue registrado pero quedó RECHAZADO automáticamente por no tener comprobante."
                        : "El gasto fue registrado pero quedó RECHAZADO automáticamente por no tener comprobante. Puede corregirlo adjuntando uno.";
                }
                else if (esAdmin)
                {
                    TempData["MensajeExito"] = "El gasto operativo se registró y aprobó correctamente.";
                }
                else
                {
                    TempData["MensajeExito"] = "El gasto se registró correctamente y quedó en estado PENDIENTE de aprobación.";
                }

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                string errorDetalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Error al guardar en base de datos: " + errorDetalle);
            }

            ViewBag.Categorias = new SelectList(await ObtenerCategoriasAsync(), "Id", "NombreCategoria", gasto.CategoriaId);
            return View(gasto);
        }

        [Authorize(Roles = "1")]
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
                    gasto.MotivoRechazo = MotivoSinComprobante;
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

        [Authorize(Roles = "1")]
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

        [Authorize(Roles = "1,2")]
        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var gasto = await _context.GastoOperativo
                .Include(g => g.Categoria)
                .Include(g => g.Usuario)
                .Include(g => g.Cita).ThenInclude(c => c!.Piscina)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gasto == null) return NotFound();

            if (!await PuedeAccederAGastoAsync(gasto.UsuarioId))
                return Forbid();

            ViewBag.VolverController = "Gastos";
            return View(gasto);
        }

        [HttpGet]
        public async Task<IActionResult> Corregir(int id)
        {
            var gasto = await _context.GastoOperativo.FindAsync(id);
            if (gasto == null)
            {
                return RedirectToAction(nameof(Index));
            }

            if (!await PuedeAccederAGastoAsync(gasto.UsuarioId))
                return Forbid();

            if (gasto.Estado != "Rechazado")
            {
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Categorias = new SelectList(await ObtenerCategoriasAsync(), "Id", "NombreCategoria", gasto.CategoriaId);
            return View(gasto);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Corregir(GastoOperativo gastoForm, IFormFile? comprobanteArchivo)
        {
            var gastoExistente = await _context.GastoOperativo.FindAsync(gastoForm.Id);
            if (gastoExistente == null) return NotFound();

            if (!await PuedeAccederAGastoAsync(gastoExistente.UsuarioId))
                return Forbid();

            ModelState.Remove(nameof(GastoOperativo.Estado));
            ModelState.Remove(nameof(GastoOperativo.Categoria));
            ModelState.Remove(nameof(GastoOperativo.Usuario));
            ModelState.Remove(nameof(GastoOperativo.Fecha));
            ModelState.Remove(nameof(GastoOperativo.Monto));
            ModelState.Remove(nameof(GastoOperativo.CategoriaId));

            gastoForm.Descripcion = gastoForm.Descripcion?.Trim();

            if (gastoForm.CategoriaId <= 0 || !await _context.CategoriaGastoOperativo.AnyAsync(c => c.Id == gastoForm.CategoriaId))
            {
                ModelState.AddModelError(nameof(GastoOperativo.CategoriaId), "Debe seleccionar una categoría válida.");
            }

            if (gastoForm.Monto <= 0 || gastoForm.Monto > 999999999.99m)
            {
                ModelState.AddModelError(nameof(GastoOperativo.Monto), "El monto debe ser mayor a cero.");
            }

            if (string.IsNullOrWhiteSpace(gastoForm.Descripcion))
            {
                ModelState.AddModelError(nameof(GastoOperativo.Descripcion), "La descripción corregida es obligatoria.");
            }
            else if (gastoForm.Descripcion.Length > 500)
            {
                ModelState.AddModelError(nameof(GastoOperativo.Descripcion), "La descripción no puede superar los 500 caracteres.");
            }

            if (comprobanteArchivo != null && comprobanteArchivo.Length > 0)
            {
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png", ".pdf" };
                var extension = Path.GetExtension(comprobanteArchivo.FileName).ToLowerInvariant();
                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError("comprobanteArchivo", "Solo se permiten archivos JPG, PNG o PDF.");
                }
            }

            if (!ModelState.IsValid)
            {
                gastoForm.MotivoRechazo = gastoExistente.MotivoRechazo;
                gastoForm.ComprobanteRuta = gastoExistente.ComprobanteRuta;
                ViewBag.Categorias = new SelectList(await ObtenerCategoriasAsync(), "Id", "NombreCategoria", gastoForm.CategoriaId);
                return View(gastoForm);
            }

            try
            {
                string valorAnterior = $"Estado: {gastoExistente.Estado} | Monto: ₡{gastoExistente.Monto:N2}";

                gastoExistente.CategoriaId = gastoForm.CategoriaId;
                gastoExistente.Monto = gastoForm.Monto;
                gastoExistente.Descripcion = gastoForm.Descripcion;

                if (gastoForm.CitaId.HasValue && gastoForm.CitaId > 0)
                {
                    var existeCita = await _context.Cita.AnyAsync(c => c.Id == gastoForm.CitaId.Value);
                    gastoExistente.CitaId = existeCita ? gastoForm.CitaId : null;
                }
                else
                {
                    gastoExistente.CitaId = null;
                }

                if (comprobanteArchivo != null && comprobanteArchivo.Length > 0)
                {
                    gastoExistente.ComprobanteRuta = await GuardarComprobanteAsync(comprobanteArchivo);
                }

                // Sin comprobante el gasto sigue rechazado.
                bool sinComprobante = string.IsNullOrEmpty(gastoExistente.ComprobanteRuta);
                gastoExistente.Estado = sinComprobante ? "Rechazado" : "Pendiente";
                gastoExistente.MotivoRechazo = sinComprobante ? MotivoSinComprobante : null;

                _context.Update(gastoExistente);
                await _context.SaveChangesAsync();

                await _bitacora.RegistrarAsync(
                    userClaims: User,
                    accion: "UPDATE",
                    tablaAfectada: "fin.GASTO_OPERATIVO",
                    registroId: gastoExistente.Id,
                    valorAnterior: valorAnterior,
                    valorNuevo: $"Estado: {gastoExistente.Estado} | Monto: ₡{gastoExistente.Monto:N2}"
                );

                if (sinComprobante)
                {
                    TempData["MensajeError"] = "El gasto sigue RECHAZADO porque no tiene comprobante. Adjunte uno para enviarlo a aprobación.";
                    return RedirectToAction(nameof(Index));
                }

                TempData["MensajeExito"] = "Corrección enviada correctamente. El gasto volvió a estado PENDIENTE.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                string errorDetalle = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                ModelState.AddModelError("", "Error al guardar la corrección: " + errorDetalle);
            }

            gastoForm.MotivoRechazo = gastoExistente.MotivoRechazo;
            gastoForm.ComprobanteRuta = gastoExistente.ComprobanteRuta;
            ViewBag.Categorias = new SelectList(await ObtenerCategoriasAsync(), "Id", "NombreCategoria", gastoForm.CategoriaId);
            return View(gastoForm);
        }

        [Authorize(Roles = "1")]
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

        private async Task<List<CategoriaGastoOperativo>> ObtenerCategoriasAsync()
        {
            await DivisionTerritorialSeeder.AsegurarCategoriasGastoOperativoAsync(_context);
            return await _context.CategoriaGastoOperativo
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();
        }

        private async Task<int?> ObtenerUsuarioActualIdAsync()
        {
            var correo = (User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name)?.Trim();
            if (string.IsNullOrEmpty(correo))
                return null;

            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Correo == correo);

            return usuario?.Id;
        }

        private async Task<bool> PuedeAccederAGastoAsync(int gastoUsuarioId)
        {
            if (User.IsInRole("1"))
                return true;

            var usuarioId = await ObtenerUsuarioActualIdAsync();
            return usuarioId.HasValue && gastoUsuarioId == usuarioId.Value;
        }

        private IActionResult RedirigirRetorno(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GuardarComprobanteAsync(IFormFile archivo)
        {
            string carpetaSubidas = Path.Combine(_env.WebRootPath, "comprobantes");
            if (!Directory.Exists(carpetaSubidas)) Directory.CreateDirectory(carpetaSubidas);

            string nombreArchivoUnique = Guid.NewGuid().ToString() + Path.GetExtension(archivo.FileName).ToLowerInvariant();
            string rutaFisica = Path.Combine(carpetaSubidas, nombreArchivoUnique);

            using (var stream = new FileStream(rutaFisica, FileMode.Create))
            {
                await archivo.CopyToAsync(stream);
            }

            return "/comprobantes/" + nombreArchivoUnique;
        }
    }
}