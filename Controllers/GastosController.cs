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
                .OrderBy(g => g.Estado == "Pendiente" ? 1 : (g.Estado == "Aprobado" ? 2 : 3))
                .ThenByDescending(g => g.Fecha)
                .ThenByDescending(g => g.Id)
                .ToListAsync();

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

                // Sin comprobante el gasto se rechaza automáticamente.
                bool sinComprobante = string.IsNullOrEmpty(gasto.ComprobanteRuta);
                if (sinComprobante)
                {
                    gasto.Estado = "Rechazado";
                    gasto.MotivoRechazo = MotivoSinComprobante;
                }

                _context.GastoOperativo.Add(gasto);
                await _context.SaveChangesAsync();

                if (sinComprobante)
                    TempData["MensajeError"] = "El gasto fue registrado pero quedó RECHAZADO automáticamente por no tener comprobante. Puede corregirlo adjuntando uno.";
                else
                    TempData["MensajeExito"] = "El gasto se registró correctamente y quedó en estado PENDIENTE de aprobación.";
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
            try
            {
                var gastoExistente = await _context.GastoOperativo.FindAsync(gastoForm.Id);
                if (gastoExistente == null) return NotFound();

                if (!await PuedeAccederAGastoAsync(gastoExistente.UsuarioId))
                    return Forbid();

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