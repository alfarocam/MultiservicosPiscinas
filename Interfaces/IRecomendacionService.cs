using MultiserviciosPiscinas.DTOs.Cotizacion;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiserviciosPiscinas.Interfaces
{
    public interface IRecomendacionService
    {
        Task<List<ProductoBusquedaDto>> ObtenerRecomendacionesAsync(int? clienteId, int limite = 3);
    }
}
