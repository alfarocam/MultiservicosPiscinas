using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Services;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class CotizacionController : Controller
    {
        private readonly ICotizacionRepository _cotizacionRepositorio;
        private readonly CotizacionPdfService _pdfService;
        private readonly IConfiguration _configuracion;

        public CotizacionController(
            ICotizacionRepository cotizacionRepositorio,
            CotizacionPdfService pdfService,
            IConfiguration configuracion)
        {
            _cotizacionRepositorio = cotizacionRepositorio;
            _pdfService = pdfService;
            _configuracion = configuracion;
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
    }
}
