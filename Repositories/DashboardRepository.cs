using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public DashboardRepository(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<DashboardDto> ObtenerDashboardAsync()
        {
            var hoy = DateTime.Today;

            //-- Rango trimestral actual --
            //Trimestre: Q1=ene-mar, Q2=abr-jun, Q3=jul-sep, Q4=oct-dic
            int trimestreActual = (hoy.Month - 1) / 3;  // 0-based quarter index
            var inicioTrimestre = new DateTime(hoy.Year, trimestreActual * 3 + 1, 1);
            var finTrimestre = inicioTrimestre.AddMonths(3).AddDays(-1);

            var inicioTrimestre_DateOnly = DateOnly.FromDateTime(inicioTrimestre);
            var finTrimestre_DateOnly = DateOnly.FromDateTime(finTrimestre);
            var inicioAnio_DateOnly = new DateOnly(hoy.Year, 1, 1);
            var finAnio_DateOnly = new DateOnly(hoy.Year, 12, 31);

            //Nombre legible del periodo
            string nombreTrimestre = $"Q{trimestreActual + 1} {hoy.Year}";
            string nombreAnio = hoy.Year.ToString();

            //------KPIs DE ARRIBA (sin rango)-----------

            //Clientes activos = total de clientes registrados
            int clientesActivos = await _context.Cliente.CountAsync();

            //Servicios este mes (por FechaApertura en el mes en curso)
            var inicioMes = DateOnly.FromDateTime(new DateTime(hoy.Year, hoy.Month, 1));
            var finMes = DateOnly.FromDateTime(new DateTime(hoy.Year, hoy.Month,
                DateTime.DaysInMonth(hoy.Year, hoy.Month)));
            int serviciosEsteMes = await _context.Servicio
                .CountAsync(s => s.FechaApertura >= inicioMes && s.FechaApertura <= finMes);

            //Facturas pendientes
            int facturasPendientes = await _context.Factura
                .CountAsync(f => f.Estado == "Pendiente");

            //Técnicos disponibles = usuarios con rol 2 (técnico) activos
            //Se usa RolId = 2 según la convención del sistema
            int tecnicosDisponibles = await _context.Usuario
                .CountAsync(u => u.RolId == 2);

            //------KPIs OPERATIVOS (trimestre)--------

            //Servicios realizados en el trimestre (cualquier estado)
            int serviciosRealizados = await _context.Servicio
                .CountAsync(s => s.FechaApertura >= inicioTrimestre_DateOnly
                              && s.FechaApertura <= finTrimestre_DateOnly);

            //Si no hay datos en el trimestre, se amplía al año actual
            bool usandoTrimestre = serviciosRealizados > 0;
            if (!usandoTrimestre)
            {
                serviciosRealizados = await _context.Servicio
                    .CountAsync(s => s.FechaApertura >= inicioAnio_DateOnly
                                  && s.FechaApertura <= finAnio_DateOnly);
            }

            //Proyectos activos en el trimestre (En ejecución o En planificación)
            int proyectosActivos = await _context.ProyectoConstruccion
                .CountAsync(p => (p.Estado == "En ejecución" || p.Estado == "En planificación")
                              && p.FechaInicio >= inicioTrimestre_DateOnly
                              && p.FechaInicio <= finTrimestre_DateOnly);

            if (!usandoTrimestre || proyectosActivos == 0)
            {
                proyectosActivos = await _context.ProyectoConstruccion
                    .CountAsync(p => p.Estado == "En ejecución" || p.Estado == "En planificación");
            }

            //Visitas técnicas (todas las citas) en el trimestre
            var inicioTrimestreDate = inicioTrimestre;
            var finTrimestreDate = finTrimestre.AddDays(1); //exclusivo
            int visitasTecnicas = await _context.Cita
                .CountAsync(c => c.FechaHora >= inicioTrimestreDate
                              && c.FechaHora < finTrimestreDate);

            if (!usandoTrimestre || visitasTecnicas == 0)
            {
                visitasTecnicas = await _context.Cita
                    .CountAsync(c => c.FechaHora.Year == hoy.Year);
            }

            string periodoKpi = usandoTrimestre ? nombreTrimestre : nombreAnio;

            //GRÁFICO 1: Servicios por mes (últimos 6 meses)
            var hace6Meses = DateOnly.FromDateTime(hoy.AddMonths(-5));
            //Agrupar en memoria para evitar problemas de traducción de DateOnly a SQL
            var serviciosUltimos6 = await _context.Servicio
                .Where(s => s.FechaApertura >= hace6Meses)
                .Select(s => s.FechaApertura)
                .ToListAsync();

            var serviciosPorMes = serviciosUltimos6
                .GroupBy(f => new { f.Year, f.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new ServiciosPorMesDto
                {
                    Mes = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yy"),
                    Cantidad = g.Count()
                })
                .ToList();

            //GRÁFICO 2: Proyectos por estado
            var estadosProyecto = await _context.ProyectoConstruccion
                .Where(p => p.Estado != null)
                .GroupBy(p => p.Estado!)
                .Select(g => new EstadoProyectoDto
                {
                    Estado = g.Key,
                    Cantidad = g.Count()
                })
                .OrderByDescending(e => e.Cantidad)
                .ToListAsync();

            //GRÁFICO 3: Visitas técnicas por técnico (top 5)
            var visitasPorTecnico = await _context.Cita
                .Include(c => c.Tecnico)
                .GroupBy(c => new { c.TecnicoId, c.Tecnico.Nombre, c.Tecnico.ApellidoPaterno })
                .Select(g => new VisitasPorTecnicoDto
                {
                    NombreTecnico = g.Key.Nombre + " " + g.Key.ApellidoPaterno,
                    Cantidad = g.Count()
                })
                .OrderByDescending(v => v.Cantidad)
                .Take(5)
                .ToListAsync();

            //ACTIVIDAD RECIENTE (últimas 5 citas)
            var actividadReciente = await _context.Cita
                .Include(c => c.Tecnico)
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .OrderByDescending(c => c.FechaHora)
                .Take(5)
                .Select(c => new ActividadRecienteDto
                {
                    Descripcion = c.Tipo,
                    NombreCliente = c.Piscina.Cliente.Usuario.Nombre + " " + c.Piscina.Cliente.Usuario.ApellidoPaterno,
                    NombreTecnico = c.Tecnico.Nombre + " " + c.Tecnico.ApellidoPaterno,
                    FechaHora = c.FechaHora,
                    Estado = c.Estado,
                    Tipo = c.Tipo
                })
                .ToListAsync();

            return new DashboardDto
            {
                ClientesActivos = clientesActivos,
                ServiciosEsteMes = serviciosEsteMes,
                FacturasPendientes = facturasPendientes,
                TecnicosDisponibles = tecnicosDisponibles,
                ServiciosRealizados = serviciosRealizados,
                ProyectosActivos = proyectosActivos,
                VisitasTecnicas = visitasTecnicas,
                PeriodoKpi = periodoKpi,
                ServiciosPorMes = serviciosPorMes,
                EstadosProyectos = estadosProyecto,
                VisitasPorTecnico = visitasPorTecnico,
                ActividadReciente = actividadReciente
            };
        }
    }
}
