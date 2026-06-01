using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Repositories
{
    public class HistorialServicioRepository : IHistorialServicioRepository
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public HistorialServicioRepository(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<List<HistorialServicioDto>> ObtenerHistorialPorClienteAsync(
            string correoUsuario,
            DateTime? fechaDesde,
            DateTime? fechaHasta)
        {
            // Buscar por correo del usuario -> cliente -> piscina -> cita -> servicio
            var query = _context.Servicios
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Tecnico)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Cliente)
                            .ThenInclude(cl => cl.Usuario)
                .Where(s => s.Cita.Piscina.Cliente.Usuario.Correo == correoUsuario);

            if (fechaDesde.HasValue)
                query = query.Where(s => s.Cita.FechaHora >= fechaDesde.Value);

            if (fechaHasta.HasValue)
                query = query.Where(s => s.Cita.FechaHora <= fechaHasta.Value.AddDays(1));

            return await query
                .OrderByDescending(s => s.Cita.FechaHora)
                .Select(s => new HistorialServicioDto
                {
                    Id = s.Id,
                    Tipo = s.Cita.Tipo,
                    FechaHora = s.Cita.FechaHora,
                    EstadoCita = s.Cita.Estado,
                    FechaApertura = s.FechaApertura,
                    FechaCierre = s.FechaCierre,
                    EstadoServicio = s.Estado,
                    TrabajoRealizado = s.TrabajoRealizado,
                    NombreTecnico = s.Cita.Tecnico.Nombre + " " + s.Cita.Tecnico.ApellidoPaterno
                })
                .ToListAsync();
        }

        public async Task<DetalleServicioDto?> ObtenerDetalleServicioAsync(
            int servicioId,
            string correoUsuario)
        {
            var servicio = await _context.Servicios
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Tecnico)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Cliente)
                            .ThenInclude(cl => cl.Usuario)
                .Include(s => s.TareaServicios)
                .Include(s => s.Inspeccions)
                .Where(s => s.Id == servicioId
                         && s.Cita.Piscina.Cliente.Usuario.Correo == correoUsuario)
                .FirstOrDefaultAsync();

            if (servicio == null)
                return null;

            var inspeccion = servicio.Inspeccions.FirstOrDefault();

            return new DetalleServicioDto
            {
                Id = servicio.Id,
                Tipo = servicio.Cita.Tipo,
                FechaHora = servicio.Cita.FechaHora,
                EstadoCita = servicio.Cita.Estado,
                Notas = servicio.Cita.Notas ?? string.Empty,
                FechaApertura = servicio.FechaApertura,
                FechaCierre = servicio.FechaCierre,
                EstadoServicio = servicio.Estado,
                TrabajoRealizado = servicio.TrabajoRealizado,
                NombreTecnico = servicio.Cita.Tecnico.Nombre + " " + servicio.Cita.Tecnico.ApellidoPaterno,
                Tareas = servicio.TareaServicios.Select(t => new TareaDto
                {
                    Descripcion = t.Descripcion,
                    Estado = t.Estado,
                    FechaAsignacion = t.FechaAsignacion,
                    FechaCompletacion = t.FechaCompletacion
                }).ToList(),
                Inspeccion = inspeccion == null ? null : new InspeccionDto
                {
                    FechaInspeccion = inspeccion.FechaInspeccion,
                    CloroPpm = inspeccion.CloroPpm,
                    Alcalinidad = inspeccion.Alcalinidad,
                    Ph = inspeccion.Ph,
                    Calcio = inspeccion.Calcio,
                    AcidoCianurico = inspeccion.AcidoCianurico,
                    Observaciones = inspeccion.Observaciones
                }
            };
        }
    }
}
