using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Interfaces;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class HistorialController : Controller
    {
        private readonly IHistorialServicioRepository _historialRepo;

        public HistorialController(IHistorialServicioRepository historialRepo)
        {
            _historialRepo = historialRepo;
        }

        // GET: /Historial
        public async Task<IActionResult> Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // El AuthController guarda el correo en ClaimTypes.Email
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("InicioSesion", "Auth");

            var historial = await _historialRepo.ObtenerHistorialPorClienteAsync(
                correo, fechaDesde, fechaHasta);

            ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");

            return View(historial);
        }

        // GET: /Historial/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("InicioSesion", "Auth");

            var detalle = await _historialRepo.ObtenerDetalleServicioAsync(id, correo);

            if (detalle == null)
                return NotFound();

            return View(detalle);
        }
    }
}
