using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Interfaces;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class FacturacionController : Controller
    {
        private readonly IFacturaRepository _facturaRepositorio;

        public FacturacionController(IFacturaRepository facturaRepositorio)
        {
            _facturaRepositorio = facturaRepositorio;
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
    }
}
