using MultiserviciosPiscinas.DTOs.Cotizacion;

namespace MultiserviciosPiscinas.Interfaces;

public interface ICarritoRepository
{
    // Obtiene los items del carrito de un cliente
    Task<List<ItemCarritoDto>> ObtenerItemsAsync(int clienteId, decimal tasaIva);

    // Agrega un producto al carrito, validando stock
    Task<(bool exito, string mensaje)> AgregarItemAsync(int clienteId, int productoId, int cantidad);

    // Elimina un producto del carrito
    Task EliminarItemAsync(int clienteId, int productoId);

    // Elimina el carrito completo (y sus items) tras concretar una compra
    Task EliminarCarritoAsync(int clienteId);
}
