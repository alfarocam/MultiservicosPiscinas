using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Interfaces;

public interface ICotizacionRepository
{
    Task<List<CategoriaProductoDto>> ObtenerCategoriasAsync();

    Task<List<ProductoBusquedaDto>> ObtenerTodosLosProductosAsync();

    Task<List<ProductoBusquedaDto>> ObtenerProductosPorCategoriaAsync(int categoriaId);

    Task<ProductoBusquedaDto?> ObtenerProductoPorIdAsync(int productoId);

    Task<List<ClienteBusquedaDto>> BuscarClientesAsync(string filtro);

    Task<ClienteBusquedaDto?> BuscarClientePorCorreoOTelefonoAsync(string valor);
    Task<int> RegistrarClienteRapidoAsync(string nombre, string apellidoPaterno, string apellidoMaterno, string correo, string telefono);

    Task<Cotizacion> CrearCotizacionAsync(int clienteId, List<ItemCarritoDto> items, decimal tasaIva, int diasVigencia);

    Task<List<CotizacionClienteListadoDto>> ObtenerCotizacionesPorClienteAsync(string correoCliente);
    Task<CotizacionClienteDetalleDto?> ObtenerDetalleCotizacionClienteAsync(int id, string correoCliente);
    Task<bool> AceptarCotizacionAsync(int id, string correoCliente);
}
