namespace MultiserviciosPiscinas.DTOs.Cotizacion;

public class CotizacionAdminListadoDto
{
    public int Id { get; set; }

    public string NumeroCotizacion { get; set; } = null!;

    public string NombreCliente { get; set; } = null!;

    public string CorreoCliente { get; set; } = null!;

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVigencia { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public int? FacturaId { get; set; }

    public string? NumeroFactura { get; set; }
}

public class CotizacionAdminDetalleViewModel
{
    public int Id { get; set; }

    public string NumeroCotizacion { get; set; } = null!;

    public string NombreCliente { get; set; } = null!;

    public string CorreoCliente { get; set; } = null!;

    public string? TelefonoCliente { get; set; }

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVigencia { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DescuentoTotal { get; set; }

    public decimal ImpuestoTotal { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public int? FacturaId { get; set; }

    public string? NumeroFactura { get; set; }

    public List<ItemCarritoDto> Lineas { get; set; } = new();
}
