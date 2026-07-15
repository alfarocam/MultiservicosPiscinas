using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.DTOs.Factura;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;
using System.Globalization;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers;

[Authorize(Roles = "3")]
public class TiendaController : Controller
{
    private readonly ICotizacionRepository _cotizacionRepositorio;
    private readonly ICarritoRepository _carritoRepositorio;
    private readonly IFacturaRepository _facturaRepositorio;
    private readonly IConfiguration _configuracion;
    private readonly PiscinasYMultiserviciosContext _context;
    private readonly ILogger<TiendaController> _logger;

    public TiendaController(
        ICotizacionRepository cotizacionRepositorio,
        ICarritoRepository carritoRepositorio,
        IFacturaRepository facturaRepositorio,
        IConfiguration configuracion,
        PiscinasYMultiserviciosContext context,
        ILogger<TiendaController> logger)
    {
        _cotizacionRepositorio = cotizacionRepositorio;
        _carritoRepositorio = carritoRepositorio;
        _facturaRepositorio = facturaRepositorio;
        _configuracion = configuracion;
        _context = context;
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerCategorias()
    {
        var categorias = await _cotizacionRepositorio.ObtenerCategoriasAsync();
        return Json(categorias);
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerProductos()
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
    public async Task<IActionResult> AgregarAlCarrito(int productoId, int cantidad)
    {
        try
        {
            var clienteId = await ObtenerClienteIdAutenticadoAsync();
            if (clienteId == null)
                return Json(new { success = false, mensaje = "No autenticado." });

            var (exito, mensaje) = await _carritoRepositorio.AgregarItemAsync(clienteId.Value, productoId, cantidad);

            if (!exito)
                return Json(new { success = false, mensaje });

            var tasaIva = decimal.Parse(_configuracion["Facturacion:TasaIva"] ?? "0.13", CultureInfo.InvariantCulture);
            var carrito = await _carritoRepositorio.ObtenerItemsAsync(clienteId.Value, tasaIva);

            return Json(new { success = true, mensaje, totalItems = carrito.Count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar la solicitud del carrito.");
            return Json(new { success = false, mensaje = "Ocurrió un error al procesar la solicitud." });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EliminarDelCarrito(int productoId)
    {
        try
        {
            var clienteId = await ObtenerClienteIdAutenticadoAsync();
            if (clienteId == null)
                return Json(new { success = false, mensaje = "No autenticado." });

            await _carritoRepositorio.EliminarItemAsync(clienteId.Value, productoId);

            return Json(new { success = true, mensaje = "Producto eliminado del carrito." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar la solicitud del carrito.");
            return Json(new { success = false, mensaje = "Ocurrió un error al procesar la solicitud." });
        }
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerCarrito()
    {
        var clienteId = await ObtenerClienteIdAutenticadoAsync();
        if (clienteId == null)
            return Json(new { items = new List<ItemCarritoDto>(), subtotal = 0, impuestoTotal = 0, total = 0, cantidad = 0 });

        var tasaIva = decimal.Parse(_configuracion["Facturacion:TasaIva"] ?? "0.13", CultureInfo.InvariantCulture);
        var carrito = await _carritoRepositorio.ObtenerItemsAsync(clienteId.Value, tasaIva);

        var subtotal = carrito.Sum(i => i.LineaSubtotal);
        var impuestoTotal = carrito.Sum(i => i.LineaImpuesto);
        var total = subtotal + impuestoTotal;

        return Json(new { items = carrito, subtotal, impuestoTotal, total, cantidad = carrito.Count });
    }

    public async Task<IActionResult> Carrito()
    {
        var clienteId = await ObtenerClienteIdAutenticadoAsync();
        if (clienteId == null)
            return RedirectToAction("InicioSesion", "Auth");

        var tasaIva = decimal.Parse(_configuracion["Facturacion:TasaIva"] ?? "0.13", CultureInfo.InvariantCulture);
        var carrito = await _carritoRepositorio.ObtenerItemsAsync(clienteId.Value, tasaIva);

        return View(carrito);
    }

    public async Task<IActionResult> Comprar()
    {
        var clienteId = await ObtenerClienteIdAutenticadoAsync();
        if (clienteId == null)
            return RedirectToAction("InicioSesion", "Auth");

        var tasaIva = decimal.Parse(_configuracion["Facturacion:TasaIva"] ?? "0.13", CultureInfo.InvariantCulture);
        var carrito = await _carritoRepositorio.ObtenerItemsAsync(clienteId.Value, tasaIva);

        if (carrito.Count == 0)
        {
            TempData["Mensaje"] = "El carrito está vacío.";
            TempData["TipoMensaje"] = "warning";
            return RedirectToAction("Carrito");
        }

        var correo = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(correo))
            return RedirectToAction("InicioSesion", "Auth");

        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);
        if (usuario == null)
            return RedirectToAction("InicioSesion", "Auth");

        var subtotal = carrito.Sum(i => i.LineaSubtotal);
        var impuestoTotal = carrito.Sum(i => i.LineaImpuesto);

        var modelo = new ComprarCarritoViewModel
        {
            NombreCliente = usuario.Nombre + " " + usuario.ApellidoPaterno,
            FechaEmision = DateOnly.FromDateTime(DateTime.Now),
            Lineas = carrito,
            Subtotal = subtotal,
            ImpuestoTotal = impuestoTotal,
            Total = subtotal + impuestoTotal
        };

        return View(modelo);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comprar(ComprarCarritoViewModel model)
    {
        var clienteId = await ObtenerClienteIdAutenticadoAsync();
        if (clienteId == null)
            return RedirectToAction("InicioSesion", "Auth");

        var tasaIva = decimal.Parse(_configuracion["Facturacion:TasaIva"] ?? "0.13", CultureInfo.InvariantCulture);
        var carrito = await _carritoRepositorio.ObtenerItemsAsync(clienteId.Value, tasaIva);

        if (carrito.Count == 0)
        {
            TempData["Mensaje"] = "El carrito está vacío.";
            TempData["TipoMensaje"] = "warning";
            return RedirectToAction("Carrito");
        }

        if (model.Comprobante == null || model.Comprobante.Length == 0)
        {
            ModelState.AddModelError(nameof(model.Comprobante), "Debe subir el comprobante de pago.");
            model.Lineas = carrito;
            model.Subtotal = carrito.Sum(i => i.LineaSubtotal);
            model.ImpuestoTotal = carrito.Sum(i => i.LineaImpuesto);
            model.Total = model.Subtotal + model.ImpuestoTotal;
            return View(model);
        }

        var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
        var extension = Path.GetExtension(model.Comprobante.FileName).ToLower();

        if (!extensionesPermitidas.Contains(extension))
        {
            ModelState.AddModelError(nameof(model.Comprobante), "Solo se permiten imágenes JPG o PNG.");
            model.Lineas = carrito;
            model.Subtotal = carrito.Sum(i => i.LineaSubtotal);
            model.ImpuestoTotal = carrito.Sum(i => i.LineaImpuesto);
            model.Total = model.Subtotal + model.ImpuestoTotal;
            return View(model);
        }

        // Validar tamaño máximo de 5MB
        const long maxFileSize = 5 * 1024 * 1024; // 5MB
        if (model.Comprobante.Length > maxFileSize)
        {
            ModelState.AddModelError(nameof(model.Comprobante), "El archivo no debe superar 5MB.");
            model.Lineas = carrito;
            model.Subtotal = carrito.Sum(i => i.LineaSubtotal);
            model.ImpuestoTotal = carrito.Sum(i => i.LineaImpuesto);
            model.Total = model.Subtotal + model.ImpuestoTotal;
            return View(model);
        }

        var correo = User.FindFirst(ClaimTypes.Email)?.Value;
        if (string.IsNullOrEmpty(correo))
            return RedirectToAction("InicioSesion", "Auth");

        var usuario = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);
        if (usuario == null)
            return RedirectToAction("InicioSesion", "Auth");

        try
        {
            // Guardar el archivo
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

            // Crear la factura desde el carrito
            var diasVencimiento = int.Parse(_configuracion["Facturacion:DiasVencimientoFactura"] ?? "30", CultureInfo.InvariantCulture);
            var factura = await _facturaRepositorio.CrearFacturaDesdeCarritoAsync(clienteId.Value, carrito, usuario.Id, comprobanteRuta, diasVencimiento);

            // Eliminar el carrito para que la próxima compra inicie uno nuevo
            await _carritoRepositorio.EliminarCarritoAsync(clienteId.Value);

            TempData["Mensaje"] = $"Compra realizada exitosamente. Factura {factura.NumeroConsecutivo}.";
            TempData["TipoMensaje"] = "success";
            TempData["FacturaDescargarId"] = factura.Id;

            return RedirectToAction("MisCompras", "Facturacion");
        }
        catch (InvalidOperationException ex)
        {
            // Error de stock
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Lineas = carrito;
            model.Subtotal = carrito.Sum(i => i.LineaSubtotal);
            model.ImpuestoTotal = carrito.Sum(i => i.LineaImpuesto);
            model.Total = model.Subtotal + model.ImpuestoTotal;
            return View(model);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al generar la factura.");
            ModelState.AddModelError(string.Empty, "Ocurrió un error al generar la factura. Intenta de nuevo.");
            model.Lineas = carrito;
            model.Subtotal = carrito.Sum(i => i.LineaSubtotal);
            model.ImpuestoTotal = carrito.Sum(i => i.LineaImpuesto);
            model.Total = model.Subtotal + model.ImpuestoTotal;
            return View(model);
        }
    }

    private async Task<int?> ObtenerClienteIdAutenticadoAsync()
    {
        var correo = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrWhiteSpace(correo))
            return null;

        return await _context.Cliente
            .Where(c => c.Usuario.Correo == correo && c.Usuario.Activo)
            .Select(c => (int?)c.Id)
            .FirstOrDefaultAsync();
    }
}
