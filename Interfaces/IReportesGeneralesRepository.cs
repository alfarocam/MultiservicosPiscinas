using MultiserviciosPiscinas.Models;
using System;
using System.Threading.Tasks;

namespace MultiserviciosPiscinas.Interfaces
{
    public interface IReportesGeneralesRepository
    {
        Task<ReporteServiciosViewModel> ObtenerReporteServiciosAsync(DateTime? fechaDesde, DateTime? fechaHasta, int? tecnicoId, string estado);
        Task<ReporteProyectosViewModel> ObtenerReporteProyectosAsync(DateTime? fechaDesde, DateTime? fechaHasta, string estado);
        Task<ReporteRentabilidadViewModel> ObtenerReporteRentabilidadAsync(int anio);
    }
}
