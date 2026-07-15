using MultiserviciosPiscinas.DTOs;

namespace MultiserviciosPiscinas.Interfaces
{
    public interface IReporteGastosOperativosRepository
    {
        Task<ReporteGastosOperativosDto> ObtenerReporteAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? categoriaId);
    }
}
