using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Interfaces;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class ReporteSatisfaccionController : Controller
    {
        private readonly IReporteSatisfaccionRepository _reporteRepo;

        public ReporteSatisfaccionController(IReporteSatisfaccionRepository reporteRepo)
        {
            _reporteRepo = reporteRepo;
        }

        // GET: /ReporteSatisfaccion
        public async Task<IActionResult> Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var reporte = await _reporteRepo.ObtenerReporteAsync(fechaDesde, fechaHasta);
            return View(reporte);
        }
    }
}
