using MultiserviciosPiscinas.DTOs;

namespace MultiserviciosPiscinas.Interfaces
{
    public interface IDashboardRepository
    {
        Task<DashboardDto> ObtenerDashboardAsync();
    }
}
