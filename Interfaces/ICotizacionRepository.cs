using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Interfaces;

public interface ICotizacionRepository
{
    Task<List<CategoriaProductoDto>> ObtenerCategoriasAsync();

    Task<List<ProductoBusquedaDto>> ObtenerProductosPorCategoriaAsync(int categoriaId);

    Task<ProductoBusquedaDto?> ObtenerProductoPorIdAsync(int productoId);

    Task<List<ClienteBusquedaDto>> BuscarClientesAsync(string filtro);

    Task<ClienteBusquedaDto?> BuscarClientePorCorreoOTelefonoAsync(string valor);

    Task<int> RegistrarClienteRapidoAsync(string nombre, string correo, string telefono);

    Task<Cotizacion> CrearCotizacionAsync(int clienteId, List<ItemCarritoDto> items, decimal tasaIva, int diasVigencia);
}
