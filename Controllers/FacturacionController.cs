using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs.Factura;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System.Globalization;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class FacturacionController : Controller
    {
        private readonly IFacturaRepository _facturaRepositorio;
        private readonly FacturaPdfService _facturaPdfService;
        private readonly IConfiguration _configuracion;
        private readonly PiscinasYMultiserviciosContext _context;
        private readonly BitacoraService _bitacora;
        private readonly ILogger<FacturacionController> _logger;

        public FacturacionController(
            IFacturaRepository facturaRepositorio,
            FacturaPdfService facturaPdfService,
            IConfiguration configuracion,
            PiscinasYMultiserviciosContext context,
            BitacoraService bitacora,
            ILogger<FacturacionController> logger)
        {
            _facturaRepositorio = facturaRepositorio;
            _facturaPdfService = facturaPdfService;
            _configuracion = configuracion;
            _context = context;
            _bitacora = bitacora;
            _logger = logger;
        }

        [Authorize(Roles = "1")]
        public async Task<IActionResult> Index()
        {
            var modelo = new FacturacionIndexViewModel
            {
                Facturas = await _facturaRepositorio.ObtenerTodasLasFacturasAsync(),
                CotizacionesFacturables = await _facturaRepositorio.ObtenerCotizacionesFacturablesAsync()
            };

            return View(modelo);
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> Facturar(int id)
        {
            var modelo = await _facturaRepositorio.ObtenerCotizacionParaFacturarAsync(id);
            if (modelo == null)
            {
                TempData["Mensaje"] = "La cotización no existe o no está disponible para facturar.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            return View(modelo);
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Facturar(int cotizacionId, FacturarCotizacionViewModel model)
        {
            var modeloActual = await _facturaRepositorio.ObtenerCotizacionParaFacturarAsync(cotizacionId);
            if (modeloActual == null)
            {
                TempData["Mensaje"] = "La cotización no existe o no está disponible para facturar.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            if (model.Comprobante == null || model.Comprobante.Length == 0)
            {
                ModelState.AddModelError(nameof(model.Comprobante), "Debe subir el comprobante de pago.");
                return View(modeloActual);
            }

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(model.Comprobante.FileName).ToLowerInvariant();

            if (!extensionesPermitidas.Contains(extension))
            {
                ModelState.AddModelError(nameof(model.Comprobante), "Solo se permiten imágenes JPG o PNG.");
                return View(modeloActual);
            }

            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("InicioSesion", "Auth");

            var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);
            if (usuario == null)
                return RedirectToAction("InicioSesion", "Auth");

            try
            {
                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "comprobantes", "facturas");
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await model.Comprobante.CopyToAsync(stream);
                }

                var comprobanteRuta = $"/comprobantes/facturas/{nombreArchivo}";

                var diasVencimiento = int.Parse(_configuracion["Facturacion:DiasVencimientoFactura"] ?? "30", CultureInfo.InvariantCulture);
                var factura = await _facturaRepositorio.CrearFacturaAsync(cotizacionId, comprobanteRuta, usuario.Id, diasVencimiento);

                await _bitacora.RegistrarAsync(
                    userClaims: User,
                    accion: "INSERT",
                    tablaAfectada: "ven.FACTURA",
                    registroId: factura.Id,
                    valorNuevo: $"Factura: {factura.NumeroConsecutivo} | Cotización: {modeloActual.NumeroCotizacion} | Total: ₡{factura.Total:N2}");

                var bytes = GenerarPdfFacturaBytes(
                    factura.NumeroConsecutivo,
                    modeloActual.NombreCliente,
                    factura.FechaEmision,
                    factura.FechaVencimiento,
                    factura.CondicionPago,
                    modeloActual.Lineas,
                    factura.Subtotal,
                    factura.ImpuestoTotal,
                    factura.Total,
                    comprobanteRuta);

                return File(bytes, "application/pdf", $"Factura-{factura.NumeroConsecutivo}.pdf");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar la factura.");
                ModelState.AddModelError(string.Empty, "Ocurrió un error al generar la factura. Intenta de nuevo.");
                return View(modeloActual);
            }
        }

        [Authorize(Roles = "1")]
        public async Task<IActionResult> DetalleAdmin(int id)
        {
            var detalle = await _facturaRepositorio.ObtenerDetalleFacturaAdminAsync(id);
            if (detalle == null)
                return NotFound();

            return View(detalle);
        }

        [Authorize(Roles = "1")]
        public async Task<IActionResult> DescargarPdfAdmin(int id)
        {
            var detalle = await _facturaRepositorio.ObtenerDetalleFacturaAdminAsync(id);
            if (detalle == null)
                return NotFound();

            var bytes = GenerarPdfFacturaBytes(
                detalle.NumeroFactura,
                detalle.NombreCliente,
                detalle.FechaEmision,
                detalle.FechaVencimiento,
                detalle.CondicionPago,
                detalle.Lineas,
                detalle.Subtotal,
                detalle.ImpuestoTotal,
                detalle.Total,
                detalle.ComprobanteSinpeRuta);

            return File(bytes, "application/pdf", $"Factura-{detalle.NumeroFactura}.pdf");
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            if (nuevoEstado != "Pagada" && nuevoEstado != "Anulada")
            {
                TempData["Mensaje"] = "El estado solicitado no es válido.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var (exito, estadoAnterior, numeroFactura) =
                await _facturaRepositorio.ActualizarEstadoFacturaAsync(id, nuevoEstado);

            if (!exito)
            {
                TempData["Mensaje"] = "No se encontró la factura indicada.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            await _bitacora.RegistrarAsync(
                userClaims: User,
                accion: "UPDATE",
                tablaAfectada: "ven.FACTURA",
                registroId: id,
                valorAnterior: $"Factura: {numeroFactura} | Estado: {estadoAnterior}",
                valorNuevo: $"Factura: {numeroFactura} | Estado: {nuevoEstado}");

            TempData["Mensaje"] = $"La factura {numeroFactura} pasó a estado {nuevoEstado}.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "3")]
        public async Task<IActionResult> MisCompras()
        {
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("InicioSesion", "Auth");

            var compras = await _facturaRepositorio.ObtenerComprasClienteAsync(correo);
            return View(compras);
        }

        [Authorize(Roles = "3")]
        public async Task<IActionResult> Detalle(int id)
        {
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("InicioSesion", "Auth");

            var detalle = await _facturaRepositorio.ObtenerDetalleCompraClienteAsync(id, correo);
            if (detalle == null)
                return NotFound();

            return View(detalle);
        }

        [Authorize(Roles = "3")]
        public async Task<IActionResult> DescargarPdf(int id)
        {
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("InicioSesion", "Auth");

            var detalle = await _facturaRepositorio.ObtenerDetalleCompraClienteAsync(id, correo);
            if (detalle == null)
                return NotFound();

            var bytes = GenerarPdfFacturaBytes(
                detalle.NumeroFactura,
                User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                detalle.FechaEmision,
                detalle.FechaVencimiento,
                detalle.CondicionPago,
                detalle.Lineas,
                detalle.Subtotal,
                detalle.ImpuestoTotal,
                detalle.Total,
                detalle.ComprobanteSinpeRuta);

            return File(bytes, "application/pdf", $"Factura-{detalle.NumeroFactura}.pdf");
        }

        private byte[] GenerarPdfFacturaBytes(
            string numeroFactura,
            string nombreCliente,
            DateOnly fechaEmision,
            DateOnly fechaVencimiento,
            string condicionPago,
            List<DetalleFacturarDto> lineas,
            decimal subtotal,
            decimal impuestoTotal,
            decimal total,
            string? comprobanteSinpeRuta)
        {
            var rutaFisicaComprobante = string.IsNullOrEmpty(comprobanteSinpeRuta)
                ? null
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", comprobanteSinpeRuta.TrimStart('/'));

            var pdfDto = new FacturaPdfDto
            {
                NumeroFactura = numeroFactura,
                NombreCliente = nombreCliente,
                FechaEmision = fechaEmision,
                FechaVencimiento = fechaVencimiento,
                CondicionPago = condicionPago,
                Lineas = lineas,
                Subtotal = subtotal,
                ImpuestoTotal = impuestoTotal,
                Total = total,
                ComprobanteRutaFisica = rutaFisicaComprobante
            };

            return _facturaPdfService.GenerarPdf(pdfDto);
        }
    }
}
