using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.DTOs.Factura;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Interfaces;

public interface IFacturaRepository
{
    Task<List<CotizacionListadoDto>> ObtenerCotizacionesFacturablesAsync();

    Task<FacturarCotizacionViewModel?> ObtenerCotizacionParaFacturarAsync(int cotizacionId);

    Task<Factura> CrearFacturaAsync(int cotizacionId, string comprobanteRuta, int usuarioId, int diasVencimiento);

    // Crear factura directamente desde el carrito de cliente (sin cotización previa)
    Task<Factura> CrearFacturaDesdeCarritoAsync(int clienteId, List<ItemCarritoDto> items, int usuarioId, string comprobanteRuta, int diasVencimiento);

    // Obtener listado de compras de un cliente
    Task<List<CompraListadoDto>> ObtenerComprasClienteAsync(string correo);

    // Obtener detalle de una compra específica, validando pertenencia al cliente
    Task<CompraDetalleViewModel?> ObtenerDetalleCompraClienteAsync(int facturaId, string correo);
}
