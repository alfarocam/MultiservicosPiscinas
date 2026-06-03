using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class HistorialController : Controller
    {
        private readonly IHistorialServicioRepository _historialRepo;
        private readonly PiscinasYMultiserviciosContext _context;

        public HistorialController(
            IHistorialServicioRepository historialRepo,
            PiscinasYMultiserviciosContext context)
        {
            _historialRepo = historialRepo;
            _context = context;
        }

        // GET: /Historial
        public async Task<IActionResult> Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            // Obtener correo desde claims
            var correo = User.FindFirst(ClaimTypes.Email)?.Value;

            if (string.IsNullOrEmpty(correo))
                return RedirectToAction("InicioSesion", "Auth");

            var historial = await _historialRepo
                .ObtenerHistorialPorClienteAsync(
                    correo,
                    fechaDesde,
                    fechaHasta);

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

            var detalle = await _historialRepo
                .ObtenerDetalleServicioAsync(id, correo);

            if (detalle == null)
                return NotFound();

            // Verificar si ya existe encuesta para este servicio
            ViewBag.YaCalificado = await _context.Encuesta
                .AnyAsync(e => e.ServicioId == id);

            return View(detalle);
        }
    }
}