using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System;
using System.Threading.Tasks;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class ReportesController : Controller
    {
        private readonly IReportesGeneralesRepository _reportesRepo;
        private readonly ReportesGeneralesExcelService _excelService;
        private readonly PiscinasYMultiserviciosContext _context;

        public ReportesController(
            IReportesGeneralesRepository reportesRepo,
            ReportesGeneralesExcelService excelService,
            PiscinasYMultiserviciosContext context)
        {
            _reportesRepo = reportesRepo;
            _excelService = excelService;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.Tecnicos = await _context.Usuario.Where(u => u.RolId == 2 || u.RolId == 3).ToListAsync(); // Asumiendo roles técnicos
            return View();
        }

        //SERVICIOS
        [HttpGet]
        public async Task<IActionResult> ObtenerReporteServicios(DateTime? fechaDesde, DateTime? fechaHasta, int? tecnicoId, string estado)
        {
            var reporte = await _reportesRepo.ObtenerReporteServiciosAsync(fechaDesde, fechaHasta, tecnicoId, estado);
            return PartialView("_TablaServicios", reporte);
        }

        public async Task<IActionResult> ExportarServiciosExcel(DateTime? fechaDesde, DateTime? fechaHasta, int? tecnicoId, string estado)
        {
            var reporte = await _reportesRepo.ObtenerReporteServiciosAsync(fechaDesde, fechaHasta, tecnicoId, estado);
            var bytes = _excelService.GenerarExcelServicios(reporte);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteServicios.xlsx");
        }

        //PROYECTOS
        [HttpGet]
        public async Task<IActionResult> ObtenerReporteProyectos(DateTime? fechaDesde, DateTime? fechaHasta, string estado)
        {
            var reporte = await _reportesRepo.ObtenerReporteProyectosAsync(fechaDesde, fechaHasta, estado);
            return PartialView("_TablaProyectos", reporte);
        }

        public async Task<IActionResult> ExportarProyectosExcel(DateTime? fechaDesde, DateTime? fechaHasta, string estado)
        {
            var reporte = await _reportesRepo.ObtenerReporteProyectosAsync(fechaDesde, fechaHasta, estado);
            var bytes = _excelService.GenerarExcelProyectos(reporte);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "ReporteProyectos.xlsx");
        }

        //RENTABILIDAD
        [HttpGet]
        public async Task<IActionResult> ObtenerReporteRentabilidad(int anio)
        {
            var reporte = await _reportesRepo.ObtenerReporteRentabilidadAsync(anio);
            return PartialView("_TablaRentabilidad", reporte);
        }

        public async Task<IActionResult> ExportarRentabilidadExcel(int anio)
        {
            var reporte = await _reportesRepo.ObtenerReporteRentabilidadAsync(anio);
            var bytes = _excelService.GenerarExcelRentabilidad(reporte);
            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"ReporteRentabilidad_{anio}.xlsx");
        }
    }
}
