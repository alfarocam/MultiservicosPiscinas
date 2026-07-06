using MultiserviciosPiscinas.DTOs.Factura;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Interfaces;

public interface IFacturaRepository
{
    Task<List<CotizacionListadoDto>> ObtenerCotizacionesFacturablesAsync();

    Task<FacturarCotizacionViewModel?> ObtenerCotizacionParaFacturarAsync(int cotizacionId);

    Task<Factura> CrearFacturaAsync(int cotizacionId, string comprobanteRuta, int usuarioId, int diasVencimiento);
}
