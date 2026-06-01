using MultiserviciosPiscinas.DTOs;

namespace MultiserviciosPiscinas.Interfaces
{
    public interface IHistorialServicioRepository
    {
        Task<List<HistorialServicioDto>> ObtenerHistorialPorClienteAsync(
            string correoUsuario,
            DateTime? fechaDesde,
            DateTime? fechaHasta);

        Task<DetalleServicioDto?> ObtenerDetalleServicioAsync(int servicioId, string correoUsuario);
    }
}
