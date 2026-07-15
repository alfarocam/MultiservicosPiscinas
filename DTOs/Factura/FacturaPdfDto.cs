namespace MultiserviciosPiscinas.DTOs.Factura;

public class FacturaPdfDto
{
    public string NumeroFactura { get; set; } = null!;

    public string NombreCliente { get; set; } = null!;

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public string CondicionPago { get; set; } = null!;

    public List<DetalleFacturarDto> Lineas { get; set; } = new();

    public decimal Subtotal { get; set; }

    public decimal ImpuestoTotal { get; set; }

    public decimal Total { get; set; }

    public string? ComprobanteRutaFisica { get; set; }
}
