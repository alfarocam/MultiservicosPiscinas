using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class ReporteEficienciaRutasController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;
        private readonly ReporteEficienciaRutasExcelService _excelService;

        public ReporteEficienciaRutasController(
            PiscinasYMultiserviciosContext context,
            ReporteEficienciaRutasExcelService excelService)
        {
            _context = context;
            _excelService = excelService;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var reporte = await ObtenerReporteAsync(fechaDesde, fechaHasta);
            return View(reporte);
        }

        [HttpGet]
        public async Task<IActionResult> ExportarExcel(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var reporte = await ObtenerReporteAsync(fechaDesde, fechaHasta);

            var bytes = _excelService.GenerarExcel(reporte);

            return File(
                bytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "ReporteEficienciaRutas.xlsx");
        }

        private async Task<ReporteEficienciaRutasDto> ObtenerReporteAsync(DateTime? fechaDesde, DateTime? fechaHasta)
        {
            var fechaDesdeFinal = fechaDesde ?? DateTime.Today.AddDays(-30);
            var fechaHastaFinal = fechaHasta ?? DateTime.Today;

            if (fechaDesdeFinal > fechaHastaFinal)
            {
                var temporal = fechaDesdeFinal;
                fechaDesdeFinal = fechaHastaFinal;
                fechaHastaFinal = temporal;
            }

            var desde = DateOnly.FromDateTime(fechaDesdeFinal.Date);
            var hasta = DateOnly.FromDateTime(fechaHastaFinal.Date);

            var rutas = await _context.RutaOptimizada
                .Include(r => r.Tecnico)
                .Include(r => r.VisitaRuta)
                    .ThenInclude(v => v.Cita)
                .Where(r => r.Fecha >= desde && r.Fecha <= hasta)
                .AsNoTracking()
                .ToListAsync();

            var detalle = rutas
                .Select(r =>
                {
                    var distanciaTramos = r.VisitaRuta.Sum(v => v.DistanciaTramoKm ?? 0);

                    var distanciaOptimizada = r.DistanciaTotalKm ?? 0;

                    var distanciaReal = distanciaTramos > 0
                        ? distanciaTramos
                        : distanciaOptimizada;

                    return new EficienciaRutaDetalleDto
                    {
                        RutaId = r.Id,
                        Fecha = r.Fecha,
                        TecnicoId = r.TecnicoId,
                        Tecnico = $"{r.Tecnico.Nombre} {r.Tecnico.ApellidoPaterno}",
                        TotalVisitas = r.VisitaRuta.Count,
                        VisitasCompletadas = r.VisitaRuta.Count(v => v.Cita.Estado == "Completada"),
                        DistanciaOptimizadaKm = distanciaOptimizada,
                        DistanciaRealKm = distanciaReal,
                        DuracionEstimadaMin = r.DuracionTotalMin ?? 0
                    };
                })
                .OrderBy(d => d.Fecha)
                .ThenBy(d => d.Tecnico)
                .ToList();

            var tecnicos = detalle
                .GroupBy(d => new
                {
                    d.TecnicoId,
                    d.Tecnico
                })
                .Select(g => new EficienciaRutaTecnicoDto
                {
                    TecnicoId = g.Key.TecnicoId,
                    Tecnico = g.Key.Tecnico,
                    TotalRutas = g.Count(),
                    TotalVisitas = g.Sum(x => x.TotalVisitas),
                    VisitasCompletadas = g.Sum(x => x.VisitasCompletadas),
                    DistanciaOptimizadaKm = g.Sum(x => x.DistanciaOptimizadaKm),
                    DistanciaRealKm = g.Sum(x => x.DistanciaRealKm),
                    DuracionTotalMin = g.Sum(x => x.DuracionEstimadaMin)
                })
                .OrderByDescending(t => t.TotalRutas)
                .ThenBy(t => t.Tecnico)
                .ToList();

            return new ReporteEficienciaRutasDto
            {
                FechaDesde = fechaDesdeFinal,
                FechaHasta = fechaHastaFinal,
                Tecnicos = tecnicos,
                Detalle = detalle
            };
        }
    }
}