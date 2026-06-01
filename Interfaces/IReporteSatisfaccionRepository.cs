using MultiserviciosPiscinas.DTOs;

namespace MultiserviciosPiscinas.Interfaces
{
    public interface IReporteSatisfaccionRepository
    {
        Task<ReporteSatisfaccionDto> ObtenerReporteAsync(
            DateTime? fechaDesde,
            DateTime? fechaHasta);
    }
}
