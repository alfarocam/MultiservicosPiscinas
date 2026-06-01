using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Repositories
{
    public class ReporteSatisfaccionRepository : IReporteSatisfaccionRepository
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public ReporteSatisfaccionRepository(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<ReporteSatisfaccionDto> ObtenerReporteAsync(
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            // DbSet correcto: _context.Encuesta (no Encuestas)
            var query = _context.Encuesta
                .Include(e => e.Servicio)
                    .ThenInclude(s => s.Cita)
                        .ThenInclude(c => c.Tecnico)
                .AsQueryable();

            if (fechaDesde.HasValue)
                query = query.Where(e => e.FechaEnvio >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(e => e.FechaEnvio <= fechaHasta.Value.AddDays(1));

            var encuestas = await query.ToListAsync();

            // Promedios por técnico
            var promediosPorTecnico = encuestas
                .GroupBy(e => e.Servicio.Cita.Tecnico.Nombre + " " + e.Servicio.Cita.Tecnico.ApellidoPaterno)
                .Select(g => new PromedioTecnicoDto
                {
                    NombreTecnico = g.Key,
                    Promedio = g.Average(e => e.Calificacion),
                    TotalEncuestas = g.Count()
                })
                .OrderByDescending(x => x.Promedio)
                .ToList();

            // Últimas 10 encuestas
            var ultimasEncuestas = encuestas
                .OrderByDescending(e => e.FechaEnvio)
                .Take(10)
                .Select(e => new EncuestaDetalleDto
                {
                    Id = e.Id,
                    ServicioId = e.ServicioId,
                    TipoServicio = e.Servicio.Cita.Tipo,
                    NombreTecnico = e.Servicio.Cita.Tecnico.Nombre + " " + e.Servicio.Cita.Tecnico.ApellidoPaterno,
                    Calificacion = e.Calificacion,
                    Comentario = e.Comentario,
                    FechaEnvio = e.FechaEnvio
                })
                .ToList();

            return new ReporteSatisfaccionDto
            {
                PromedioGeneral = encuestas.Any() ? encuestas.Average(e => e.Calificacion) : 0,
                TotalEncuestas = encuestas.Count,
                TotalCriticas = encuestas.Count(e => e.Calificacion <= 2),
                PromediosPorTecnico = promediosPorTecnico,
                UltimasEncuestas = ultimasEncuestas,
                FechaDesde = fechaDesde,
                FechaHasta = fechaHasta
            };
        }
    }
}
