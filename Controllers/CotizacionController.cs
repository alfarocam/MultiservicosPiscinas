using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Services;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class CotizacionController : Controller
    {
        private readonly ICotizacionRepository _cotizacionRepositorio;
        private readonly CotizacionPdfService _pdfService;
        private readonly IConfiguration _configuracion;
        private readonly BitacoraService _bitacora;
        private readonly Generales _generales;
        private readonly IWebHostEnvironment _entornoWeb;
        private readonly ILogger<CotizacionController> _logger;

        public CotizacionController(
            ICotizacionRepository cotizacionRepositorio,
            CotizacionPdfService pdfService,
            IConfiguration configuracion,
            BitacoraService bitacora,
            Generales generales,
            IWebHostEnvironment entornoWeb,
            ILogger<CotizacionController> logger)
        {
            _cotizacionRepositorio = cotizacionRepositorio;
            _pdfService = pdfService;
            _configuracion = configuracion;
            _bitacora = bitacora;
            _generales = generales;
            _entornoWeb = entornoWeb;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var cotizaciones = await _cotizacionRepositorio.ObtenerTodasLasCotizacionesAsync();
            return View(cotizaciones);
        }

        public IActionResult Crear()
        {
            return View();
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var detalle = await _cotizacionRepositorio.ObtenerDetalleCotizacionAdminAsync(id);
            if (detalle == null)
                return NotFound();

            return View(detalle);
        }

        public async Task<IActionResult> DescargarPdf(int id)
        {
            var detalle = await _cotizacionRepositorio.ObtenerDetalleCotizacionAdminAsync(id);
            if (detalle == null)
                return NotFound();

            var pdfDto = new CotizacionPdfDto
            {
                NumeroCotizacion = detalle.NumeroCotizacion,
                NombreCliente = detalle.NombreCliente,
                CorreoCliente = detalle.CorreoCliente,
                TelefonoCliente = detalle.TelefonoCliente ?? string.Empty,
                FechaEmision = detalle.FechaEmision,
                FechaVigencia = detalle.FechaVigencia,
                NumeroSinpe = _configuracion["DatosContacto:NumeroSinpe"],
                Lineas = detalle.Lineas,
                Subtotal = detalle.Subtotal,
                ImpuestoTotal = detalle.ImpuestoTotal,
                Total = detalle.Total
            };

            var bytes = _pdfService.GenerarPdf(pdfDto);
            return File(bytes, "application/pdf", $"Cotizacion-{detalle.NumeroCotizacion}.pdf");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id, string nuevoEstado)
        {
            if (nuevoEstado != "Aceptada" && nuevoEstado != "Rechazada")
            {
                TempData["Mensaje"] = "El estado solicitado no es válido.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            var (exito, estadoAnterior) = await _cotizacionRepositorio.ActualizarEstadoCotizacionAsync(id, nuevoEstado);
            if (!exito)
            {
                TempData["Mensaje"] = "La cotización no existe, ya fue procesada o cuenta con una factura emitida.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            await _bitacora.RegistrarAsync(
                userClaims: User,
                accion: "UPDATE",
                tablaAfectada: "ven.COTIZACION",
                registroId: id,
                valorAnterior: $"Cotización: COT-{id:D5} | Estado: {estadoAnterior}",
                valorNuevo: $"Cotización: COT-{id:D5} | Estado: {nuevoEstado}");

            TempData["Mensaje"] = $"La cotización COT-{id:D5} pasó a estado {nuevoEstado}.";
            TempData["TipoMensaje"] = "success";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerCategorias()
        {
            var categorias = await _cotizacionRepositorio.ObtenerCategoriasAsync();
            return Json(categorias);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerTodosLosProductos()
        {
            var productos = await _cotizacionRepositorio.ObtenerTodosLosProductosAsync();
            return Json(productos);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerProductosPorCategoria(int categoriaId)
        {
            var productos = await _cotizacionRepositorio.ObtenerProductosPorCategoriaAsync(categoriaId);
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

                var producto = await _cotizacionRepositorio.ObtenerProductoPorIdAsync(productoId);

                if (producto == null)
                    return Json(new { success = false, mensaje = "Producto no encontrado." });

                var carrito = HttpContext.Session.ObtenerCarrito();
                var itemExistente = carrito.FirstOrDefault(i => i.ProductoId == productoId);

                var tasaIva = decimal.Parse(_configuracion["Facturacion:TasaIva"] ?? "0.13", CultureInfo.InvariantCulture);

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
                _logger.LogError(ex, "Error al procesar la solicitud del carrito de cotización.");
                return Json(new { success = false, mensaje = "Ocurrió un error al procesar la solicitud." });
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
                _logger.LogError(ex, "Error al procesar la solicitud del carrito de cotización.");
                return Json(new { success = false, mensaje = "Ocurrió un error al procesar la solicitud." });
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
        public async Task<IActionResult> BuscarClientes(string filtro)
        {
            var clientes = await _cotizacionRepositorio.BuscarClientesAsync(filtro ?? "");
            return Json(clientes);
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

                model.NombreCliente = (model.NombreCliente ?? string.Empty).Trim();
                model.ApellidoPaterno = (model.ApellidoPaterno ?? string.Empty).Trim();
                model.ApellidoMaterno = (model.ApellidoMaterno ?? string.Empty).Trim();
                model.CorreoCliente = (model.CorreoCliente ?? string.Empty).Trim();
                model.TelefonoCliente = Generales.NormalizarTelefono(model.TelefonoCliente);

                // Validar datos del cliente
                if (string.IsNullOrWhiteSpace(model.NombreCliente) ||
                    string.IsNullOrWhiteSpace(model.ApellidoPaterno) ||
                    string.IsNullOrWhiteSpace(model.ApellidoMaterno) ||
                    string.IsNullOrWhiteSpace(model.CorreoCliente) ||
                    string.IsNullOrWhiteSpace(model.TelefonoCliente))
                {
                    TempData["Mensaje"] = "Todos los datos del cliente son requeridos.";
                    TempData["TipoMensaje"] = "danger";
                    return RedirectToAction("Crear");
                }

                if (!Regex.IsMatch(
                        model.CorreoCliente,
                        @"^[a-zA-Z0-9._%+-]+@(gmail\.com|hotmail\.com|yahoo\.com|outlook\.com|icloud\.com|live\.com|piscinas\.com|multiservicios\.com|test\.com|correo\.com|ejemplo\.com)$",
                        RegexOptions.IgnoreCase))
                {
                    TempData["Mensaje"] = "Ingresá un correo válido (@gmail.com, @hotmail.com, @yahoo.com, @outlook.com o institucional).";
                    TempData["TipoMensaje"] = "danger";
                    return RedirectToAction("Crear");
                }

                if (!Regex.IsMatch(model.TelefonoCliente, @"^\d{8}$"))
                {
                    TempData["Mensaje"] = "El teléfono debe contener exactamente 8 dígitos.";
                    TempData["TipoMensaje"] = "danger";
                    return RedirectToAction("Crear");
                }

                var telefonoFormateado = Generales.FormatearTelefono(model.TelefonoCliente);

                // Buscar o crear cliente (por correo o teléfono para evitar duplicados)
                int clienteId;
                bool esClienteNuevo = false;
                string? contrasenaTemporal = null;

                var clienteExistente = await _cotizacionRepositorio.BuscarClientePorCorreoOTelefonoAsync(model.CorreoCliente)
                    ?? await _cotizacionRepositorio.BuscarClientePorCorreoOTelefonoAsync(telefonoFormateado);

                if (clienteExistente != null && clienteExistente.Encontrado)
                {
                    clienteId = clienteExistente.ClienteId;
                }
                else
                {
                    var registroCliente = await _cotizacionRepositorio.RegistrarClienteRapidoAsync(
                        model.NombreCliente,
                        model.ApellidoPaterno,
                        model.ApellidoMaterno,
                        model.CorreoCliente,
                        telefonoFormateado
                    );
                    clienteId = registroCliente.ClienteId;
                    contrasenaTemporal = registroCliente.ContrasenaTemporal;
                    esClienteNuevo = true;
                }

                // Crear cotización
                var tasaIva = decimal.Parse(_configuracion["Facturacion:TasaIva"] ?? "0.13", CultureInfo.InvariantCulture);
                var diasVigencia = int.Parse(_configuracion["Facturacion:DiasVigenciaCotizacion"] ?? "5", CultureInfo.InvariantCulture);

                var cotizacion = await _cotizacionRepositorio.CrearCotizacionAsync(clienteId, carrito, tasaIva, diasVigencia);
                var numeroCotizacion = $"COT-{cotizacion.Id:D5}";

                // Limpiar carrito
                HttpContext.Session.LimpiarCarrito();

                if (esClienteNuevo && !string.IsNullOrEmpty(contrasenaTemporal))
                {
                    bool correoEnviado = await EnviarCorreoCuentaCreadaAsync(
                        $"{model.NombreCliente} {model.ApellidoPaterno}".Trim(),
                        model.CorreoCliente,
                        contrasenaTemporal,
                        numeroCotizacion);

                    if (correoEnviado)
                    {
                        TempData["Mensaje"] = $"Cotización {numeroCotizacion} generada correctamente. Se creó la cuenta de usuario y se envió un correo de confirmación a {model.CorreoCliente} con su contraseña temporal.";
                        TempData["TipoMensaje"] = "success";
                        TempData["CorreoConfirmacion"] = $"Se envió un correo a {model.CorreoCliente} con las credenciales de acceso para el nuevo usuario.";
                        TempData["CorreoConfirmacionTipo"] = "success";
                    }
                    else
                    {
                        TempData["Mensaje"] = $"Cotización {numeroCotizacion} generada y cuenta creada para {model.CorreoCliente}, pero no se pudo enviar el correo con la contraseña temporal.";
                        TempData["TipoMensaje"] = "warning";
                        TempData["CorreoConfirmacion"] = $"La cotización {numeroCotizacion} se creó, pero no se pudo enviar el correo con la contraseña temporal a {model.CorreoCliente}.";
                        TempData["CorreoConfirmacionTipo"] = "warning";
                    }
                }
                else
                {
                    TempData["Mensaje"] = $"Cotización {numeroCotizacion} generada correctamente.";
                    TempData["TipoMensaje"] = "success";
                }

                TempData["CotizacionDescargarId"] = cotizacion.Id;
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al generar la cotización.");
                TempData["Mensaje"] = "Ocurrió un error al generar la cotización. Intenta de nuevo.";
                TempData["TipoMensaje"] = "danger";
                return RedirectToAction("Crear");
            }
        }

        private async Task<bool> EnviarCorreoCuentaCreadaAsync(
            string nombreCompleto,
            string correoDestino,
            string contrasenaTemporal,
            string numeroCotizacion)
        {
            try
            {
                string rutaHtml = Path.Combine(
                    _entornoWeb.ContentRootPath,
                    "Template",
                    "CuentaCreadaCotizacion.html");

                if (!System.IO.File.Exists(rutaHtml))
                {
                    _logger.LogError("No se encontró la plantilla de correo de cuenta creada en cotización en {Ruta}", rutaHtml);
                    return false;
                }

                string contenidoHtml = await System.IO.File.ReadAllTextAsync(rutaHtml);

                string htmlFinal = contenidoHtml
                    .Replace("{{NOMBRE_USUARIO}}", WebUtility.HtmlEncode(nombreCompleto))
                    .Replace("{{NUMERO_COTIZACION}}", WebUtility.HtmlEncode(numeroCotizacion))
                    .Replace("{{CORREO_USUARIO}}", WebUtility.HtmlEncode(correoDestino))
                    .Replace("{{CONTRASENA_TEMPORAL}}", WebUtility.HtmlEncode(contrasenaTemporal));

                _generales.EnviarCorreo(
                    correoDestino,
                    $"Tu cuenta y cotización {numeroCotizacion} — Multiservicios y Piscinas",
                    htmlFinal);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falló el envío del correo de credenciales al crear cuenta en cotización para {Correo}", correoDestino);
                return false;
            }
        }
    }
}
