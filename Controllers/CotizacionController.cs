using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.DTOs.Factura;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class CotizacionController : Controller
    {
        private readonly ICotizacionRepository _cotizacionRepositorio;
        private readonly IFacturaRepository _facturaRepositorio;
        private readonly CotizacionPdfService _pdfService;
        private readonly FacturaPdfService _facturaPdfService;
        private readonly IConfiguration _configuracion;
        private readonly PiscinasYMultiserviciosContext _context;

        public CotizacionController(
            ICotizacionRepository cotizacionRepositorio,
            IFacturaRepository facturaRepositorio,
            CotizacionPdfService pdfService,
            FacturaPdfService facturaPdfService,
            IConfiguration configuracion,
            PiscinasYMultiserviciosContext context)
        {
            _cotizacionRepositorio = cotizacionRepositorio;
            _facturaRepositorio = facturaRepositorio;
            _pdfService = pdfService;
            _facturaPdfService = facturaPdfService;
            _configuracion = configuracion;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var cotizaciones = await _facturaRepositorio.ObtenerCotizacionesFacturablesAsync();
            return View(cotizaciones);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> BuscarProductos(string filtro)
        {
            var productos = await _cotizacionRepositorio.BuscarProductosAsync(filtro ?? "");
            return Json(productos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AgregarAlCarrito(int productoId, decimal cantidad)
        {
            try
            {
                if (cantidad <= 0)
                    return Json(new { success = false, mensaje = "La cantidad debe ser mayor a 0." });

                var productos = await _cotizacionRepositorio.BuscarProductosAsync("");
                var producto = productos.FirstOrDefault(p => p.Id == productoId);

                if (producto == null)
                    return Json(new { success = false, mensaje = "Producto no encontrado." });

                var carrito = HttpContext.Session.ObtenerCarrito();
                var itemExistente = carrito.FirstOrDefault(i => i.ProductoId == productoId);

                var tasaIva = decimal.Parse(_configuracion["Facturacion:TasaIva"] ?? "0.13");

                if (itemExistente != null)
                {
                    itemExistente.Cantidad += cantidad;
                }
                else
                {
                    var nuevoItem = new ItemCarritoDto
                    {
                        ProductoId = productoId,
                        Nombre = producto.Nombre,
                        Descripcion = producto.Descripcion,
                        PrecioUnitario = producto.Precio,
                        Cantidad = cantidad,
                        LineaImpuesto = (producto.Precio * cantidad) * tasaIva
                    };
                    carrito.Add(nuevoItem);
                }

                // Recalcular IVA para todos los items
                foreach (var item in carrito)
                {
                    item.LineaImpuesto = (item.PrecioUnitario * item.Cantidad) * tasaIva;
                }

                HttpContext.Session.GuardarCarrito(carrito);

                return Json(new { success = true, mensaje = "Producto agregado al carrito.", totalItems = carrito.Count });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = $"Error: {ex.Message}" });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult EliminarDelCarrito(int productoId)
        {
            try
            {
                var carrito = HttpContext.Session.ObtenerCarrito();
                carrito.RemoveAll(i => i.ProductoId == productoId);
                HttpContext.Session.GuardarCarrito(carrito);

                return Json(new { success = true, mensaje = "Producto eliminado del carrito." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, mensaje = $"Error: {ex.Message}" });
            }
        }

        [HttpGet]
        public IActionResult ObtenerCarrito()
        {
            var carrito = HttpContext.Session.ObtenerCarrito();
            var subtotal = carrito.Sum(i => i.LineaSubtotal);
            var impuestoTotal = carrito.Sum(i => i.LineaImpuesto);
            var total = subtotal + impuestoTotal;

            return Json(new
            {
                items = carrito,
                subtotal,
                impuestoTotal,
                total,
                cantidad = carrito.Count
            });
        }

        [HttpGet]
        public async Task<IActionResult> BuscarCliente(string valor)
        {
            if (string.IsNullOrWhiteSpace(valor))
                return Json(new { encontrado = false });

            var cliente = await _cotizacionRepositorio.BuscarClientePorCorreoOTelefonoAsync(valor);
            if (cliente != null)
                return Json(cliente);

            return Json(new { encontrado = false });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerarCotizacion(CotizacionCrearViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                    return RedirectToAction("Crear");

                var carrito = HttpContext.Session.ObtenerCarrito();
                if (carrito.Count == 0)
                {
                    TempData["Mensaje"] = "El carrito está vacío.";
                    TempData["TipoMensaje"] = "warning";
                    return RedirectToAction("Crear");
                }

                // Validar datos del cliente
                if (string.IsNullOrWhiteSpace(model.NombreCliente) ||
                    string.IsNullOrWhiteSpace(model.CorreoCliente) ||
                    string.IsNullOrWhiteSpace(model.TelefonoCliente))
                {
                    TempData["Mensaje"] = "Todos los datos del cliente son requeridos.";
                    TempData["TipoMensaje"] = "danger";
                    return RedirectToAction("Crear");
                }

                // Buscar o crear cliente
                int clienteId;
                var clienteExistente = await _cotizacionRepositorio.BuscarClientePorCorreoOTelefonoAsync(model.CorreoCliente);

                if (clienteExistente != null && clienteExistente.Encontrado)
                {
                    clienteId = clienteExistente.ClienteId;
                }
                else
                {
                    clienteId = await _cotizacionRepositorio.RegistrarClienteRapidoAsync(
                        model.NombreCliente,
                        model.CorreoCliente,
                        model.TelefonoCliente
                    );
                }

                // Crear cotización
                var tasaIva = decimal.Parse(_configuracion["Facturacion:TasaIva"] ?? "0.13");
                var diasVigencia = int.Parse(_configuracion["Facturacion:DiasVigenciaCotizacion"] ?? "5");

                var cotizacion = await _cotizacionRepositorio.CrearCotizacionAsync(clienteId, carrito, tasaIva, diasVigencia);

                // Armar datos para PDF
                var numeroCotizacion = $"COT-{cotizacion.Id:D5}";
                var pdfDto = new CotizacionPdfDto
                {
                    NumeroCotizacion = numeroCotizacion,
                    NombreCliente = model.NombreCliente,
                    CorreoCliente = model.CorreoCliente,
                    TelefonoCliente = model.TelefonoCliente,
                    FechaEmision = cotizacion.FechaEmision,
                    FechaVigencia = cotizacion.FechaVigencia,
                    NumeroSinpe = _configuracion["DatosContacto:NumeroSinpe"],
                    Lineas = carrito,
                    Subtotal = cotizacion.Subtotal,
                    ImpuestoTotal = cotizacion.ImpuestoTotal,
                    Total = cotizacion.Total
                };

                // Generar PDF
                var bytes = _pdfService.GenerarPdf(pdfDto);

                // Limpiar carrito
                HttpContext.Session.LimpiarCarrito();

                // Descargar PDF
                return File(bytes, "application/pdf", $"Cotizacion-{numeroCotizacion}.pdf");
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Error al generar la cotización: {ex.Message}";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction("Crear");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Facturar(int id)
        {
            var modelo = await _facturaRepositorio.ObtenerCotizacionParaFacturarAsync(id);
            if (modelo == null)
            {
                TempData["Mensaje"] = "La cotización no existe o no está disponible para facturar.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction("Index");
            }

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Facturar(int cotizacionId, FacturarCotizacionViewModel model)
        {
            var modeloActual = await _facturaRepositorio.ObtenerCotizacionParaFacturarAsync(cotizacionId);
            if (modeloActual == null)
            {
                TempData["Mensaje"] = "La cotización no existe o no está disponible para facturar.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction("Index");
            }

            if (model.Comprobante == null || model.Comprobante.Length == 0)
            {
                ModelState.AddModelError(nameof(model.Comprobante), "Debe subir el comprobante de pago.");
                return View(modeloActual);
            }

            var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(model.Comprobante.FileName).ToLower();

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

                var diasVencimiento = int.Parse(_configuracion["Facturacion:DiasVencimientoFactura"] ?? "30");
                var factura = await _facturaRepositorio.CrearFacturaAsync(cotizacionId, comprobanteRuta, usuario.Id, diasVencimiento);

                var pdfDto = new FacturaPdfDto
                {
                    NumeroFactura = factura.NumeroConsecutivo,
                    NombreCliente = modeloActual.NombreCliente,
                    FechaEmision = factura.FechaEmision,
                    FechaVencimiento = factura.FechaVencimiento,
                    CondicionPago = factura.CondicionPago,
                    Lineas = modeloActual.Lineas,
                    Subtotal = factura.Subtotal,
                    ImpuestoTotal = factura.ImpuestoTotal,
                    Total = factura.Total
                };

                var bytes = _facturaPdfService.GenerarPdf(pdfDto);

                return File(bytes, "application/pdf", $"Factura-{factura.NumeroConsecutivo}.pdf");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Error al generar la factura: {ex.Message}");
                return View(modeloActual);
            }
        }
    }
}
