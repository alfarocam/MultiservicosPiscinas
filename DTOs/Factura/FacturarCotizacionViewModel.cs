namespace MultiserviciosPiscinas.DTOs.Factura;

public class FacturarCotizacionViewModel
{
    public int CotizacionId { get; set; }

    public string NumeroCotizacion { get; set; } = null!;

    public string NombreCliente { get; set; } = null!;

    public DateOnly FechaEmision { get; set; }

    public List<DetalleFacturarDto> Lineas { get; set; } = new();

    public decimal Subtotal { get; set; }

    public decimal ImpuestoTotal { get; set; }

    public decimal Total { get; set; }

    public IFormFile? Comprobante { get; set; }
}
