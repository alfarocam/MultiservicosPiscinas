using MultiserviciosPiscinas.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MultiserviciosPiscinas.Interfaces
{
    public interface IRecomendacionService
    {
        Task<List<Producto>> ObtenerRecomendacionesAsync(int? clienteId, int limite = 3);
    }
}
