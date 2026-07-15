using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Interfaces;

public interface ICotizacionRepository
{
    Task<List<ProductoBusquedaDto>> BuscarProductosAsync(string filtro);

    Task<ClienteBusquedaDto?> BuscarClientePorCorreoOTelefonoAsync(string valor);

    Task<int> RegistrarClienteRapidoAsync(string nombre, string correo, string telefono);

    Task<Cotizacion> CrearCotizacionAsync(int clienteId, List<ItemCarritoDto> items, decimal tasaIva, int diasVigencia);
}
