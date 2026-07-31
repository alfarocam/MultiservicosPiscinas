using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiserviciosPiscinas.Repositories
{
    public class ReportesGeneralesRepository : IReportesGeneralesRepository
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public ReportesGeneralesRepository(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<ReporteServiciosViewModel> ObtenerReporteServiciosAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? tecnicoId, string estado)
        {
            var query = _context.Cita
                .Include(c => c.Piscina)
                .ThenInclude(p => p.Cliente)
                .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Tecnico)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(c => c.FechaHora.Date >= fechaDesde.Value.Date);
            
            if (fechaHasta.HasValue)
                query = query.Where(c => c.FechaHora.Date <= fechaHasta.Value.Date);

            if (tecnicoId.HasValue)
                query = query.Where(c => c.TecnicoId == tecnicoId.Value);

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(c => c.Estado == estado);

            var citas = await query.ToListAsync();

            var viewModel = new ReporteServiciosViewModel
            {
                Servicios = citas.Select(c => new ReporteDetalleServicioDto
                {
                    Id = c.Id,
                    Cliente = c.Piscina?.Cliente?.Usuario?.Nombre + " " + c.Piscina?.Cliente?.Usuario?.ApellidoPaterno,
                    TipoServicio = c.Tipo,
                    Tecnico = c.Tecnico?.Nombre + " " + c.Tecnico?.ApellidoPaterno,
                    Estado = c.Estado,
                    FechaHora = c.FechaHora
                }).ToList()
            };

            return viewModel;
        }

        public async Task<ReporteProyectosViewModel> ObtenerReporteProyectosAsync(DateTime? fechaDesde, DateTime? fechaHasta, string estado)
        {
            var query = _context.ProyectoConstruccion
                .Include(p => p.Cliente)
                .ThenInclude(cl => cl.Usuario)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(p => p.FechaInicio >= DateOnly.FromDateTime(fechaDesde.Value.Date));
            
            if (fechaHasta.HasValue)
                query = query.Where(p => p.FechaInicio <= DateOnly.FromDateTime(fechaHasta.Value.Date));

            if (!string.IsNullOrEmpty(estado))
                query = query.Where(p => p.Estado == estado);

            var proyectos = await query.ToListAsync();

            var viewModel = new ReporteProyectosViewModel
            {
                Proyectos = proyectos.Select(p => new DetalleProyectoDto
                {
                    Id = p.Id,
                    Nombre = p.Nombre ?? "Sin nombre",
                    Cliente = p.Cliente?.Usuario?.Nombre + " " + p.Cliente?.Usuario?.ApellidoPaterno,
                    Estado = p.Estado ?? "Pendiente",
                    FechaInicio = p.FechaInicio,
                    FechaFinEstimada = p.FechaFinEstimada,
                    Presupuesto = p.Presupuesto
                }).ToList()
            };

            return viewModel;
        }

        public async Task<ReporteRentabilidadViewModel> ObtenerReporteRentabilidadAsync(int anio)
        {
            //Obtener ingresos del año
            var ingresos = await _context.Factura
                .Where(f => f.FechaEmision.Year == anio && f.Estado != "Anulada")
                .ToListAsync();

            //Obtener gastos del año
            var gastos = await _context.GastoOperativo
                .Where(g => g.Fecha.Year == anio && g.Estado == "Aprobado")
                .ToListAsync();

            var viewModel = new ReporteRentabilidadViewModel();

            //Agrupar por mes
            for (int i = 1; i <= 12; i++)
            {
                var ingresoMes = ingresos.Where(f => f.FechaEmision.Month == i).Sum(f => f.Total);
                var gastoMes = gastos.Where(g => g.Fecha.Month == i).Sum(g => g.Monto);

                if (ingresoMes > 0 || gastoMes > 0)
                {
                    viewModel.Detalles.Add(new DetalleRentabilidadDto
                    {
                        Mes = new DateTime(anio, i, 1).ToString("MMMM yyyy"),
                        Ingresos = ingresoMes,
                        Gastos = gastoMes
                    });
                }
            }

            viewModel.TotalIngresos = viewModel.Detalles.Sum(d => d.Ingresos);
            viewModel.TotalGastos = viewModel.Detalles.Sum(d => d.Gastos);

            return viewModel;
        }
    }
}
