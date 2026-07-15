using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.DTOs.Factura;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Services;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class FacturacionController : Controller
    {
        private readonly IFacturaRepository _facturaRepositorio;
        private readonly FacturaPdfService _facturaPdfService;

        public FacturacionController(IFacturaRepository facturaRepositorio, FacturaPdfService facturaPdfService)
        {
            _facturaRepositorio = facturaRepositorio;
            _facturaPdfService = facturaPdfService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Crear()
        {
            return View();
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

            var rutaFisicaComprobante = string.IsNullOrEmpty(detalle.ComprobanteSinpeRuta)
                ? null
                : Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", detalle.ComprobanteSinpeRuta.TrimStart('/'));

            var pdfDto = new FacturaPdfDto
            {
                NumeroFactura = detalle.NumeroFactura,
                NombreCliente = User.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                FechaEmision = detalle.FechaEmision,
                FechaVencimiento = detalle.FechaVencimiento,
                CondicionPago = detalle.CondicionPago,
                Lineas = detalle.Lineas,
                Subtotal = detalle.Subtotal,
                ImpuestoTotal = detalle.ImpuestoTotal,
                Total = detalle.Total,
                ComprobanteRutaFisica = rutaFisicaComprobante
            };

            var bytes = _facturaPdfService.GenerarPdf(pdfDto);

            return File(bytes, "application/pdf", $"Factura-{detalle.NumeroFactura}.pdf");
        }
    }
}
