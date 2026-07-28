using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Interfaces;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class CotizacionesClienteController : Controller
    {
        private readonly ICotizacionRepository _cotizacionRepositorio;
        private readonly ILogger<CotizacionesClienteController> _logger;

        public CotizacionesClienteController(
            ICotizacionRepository cotizacionRepositorio,
            ILogger<CotizacionesClienteController> logger)
        {
            _cotizacionRepositorio = cotizacionRepositorio;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var correo = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(correo))
            {
                return RedirectToAction("InicioSesion", "Auth");
            }

            var cotizaciones = await _cotizacionRepositorio.ObtenerCotizacionesPorClienteAsync(correo);
            return View(cotizaciones);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var correo = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(correo))
            {
                return RedirectToAction("InicioSesion", "Auth");
            }

            var cotizacion = await _cotizacionRepositorio.ObtenerDetalleCotizacionClienteAsync(id, correo);
            if (cotizacion == null)
            {
                TempData["Mensaje"] = "Cotización no encontrada.";
                TempData["TipoMensaje"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            return View(cotizacion);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Aceptar(int id)
        {
            var correo = User.FindFirst(ClaimTypes.Name)?.Value;
            if (string.IsNullOrEmpty(correo))
            {
                return RedirectToAction("InicioSesion", "Auth");
            }

            try
            {
                var exito = await _cotizacionRepositorio.AceptarCotizacionAsync(id, correo);
                if (exito)
                {
                    TempData["MensajeExito"] = "La cotización ha sido aceptada correctamente.";
                }
                else
                {
                    TempData["MensajeError"] = "No se pudo aceptar la cotización. Puede que ya haya sido procesada.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al aceptar cotización.");
                TempData["MensajeError"] = "Ocurrió un error inesperado al aceptar la cotización.";
            }

            return RedirectToAction(nameof(Detalle), new { id });
        }
    }
}
