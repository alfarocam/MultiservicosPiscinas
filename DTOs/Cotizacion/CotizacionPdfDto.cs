namespace MultiserviciosPiscinas.DTOs.Cotizacion;

public class CotizacionPdfDto
{
    public string NumeroCotizacion { get; set; } = null!;

    public string NombreCliente { get; set; } = null!;

    public string CorreoCliente { get; set; } = null!;

    public string TelefonoCliente { get; set; } = null!;

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVigencia { get; set; }

    public string? NumeroSinpe { get; set; }

    public List<ItemCarritoDto> Lineas { get; set; } = new();

    public decimal Subtotal { get; set; }

    public decimal ImpuestoTotal { get; set; }

    public decimal Total { get; set; }
}
