using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class ReporteGastosOperativosController : Controller
    {
        private readonly IReporteGastosOperativosRepository _reporteRepo;
        private readonly GastoOperativoExcelService _excelService;
        private readonly PiscinasYMultiserviciosContext _context;

        public ReporteGastosOperativosController(
            IReporteGastosOperativosRepository reporteRepo,
            GastoOperativoExcelService excelService,
            PiscinasYMultiserviciosContext context)
        {
            _reporteRepo = reporteRepo;
            _excelService = excelService;
            _context = context;
        }

        public async Task<IActionResult> Index(DateTime? fechaDesde, DateTime? fechaHasta, int? categoriaId)
        {
            ViewBag.Categorias = await _context.CategoriaGastoOperativo.ToListAsync();
            ViewBag.FechaDesde = fechaDesde?.ToString("yyyy-MM-dd");
            ViewBag.FechaHasta = fechaHasta?.ToString("yyyy-MM-dd");
            ViewBag.CategoriaId = categoriaId;
            var reporte = await _reporteRepo.ObtenerReporteAsync(fechaDesde, fechaHasta, categoriaId);
            return View(reporte);
        }

        public async Task<IActionResult> ExportarExcel(DateTime? fechaDesde, DateTime? fechaHasta, int? categoriaId)
        {
            var reporte = await _reporteRepo.ObtenerReporteAsync(fechaDesde, fechaHasta, categoriaId);
            var bytes = _excelService.GenerarExcel(reporte);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ResumenGastosOperativos.xlsx");
        }
    }
}
