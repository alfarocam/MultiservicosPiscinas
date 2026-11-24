namespace MultiserviciosPiscinas.DTOs.Factura;

public class FacturaAdminListadoDto
{
    public int Id { get; set; }

    public string NumeroFactura { get; set; } = null!;

    public string NombreCliente { get; set; } = null!;

    public string CorreoCliente { get; set; } = null!;

    public string? NumeroCotizacion { get; set; }

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public string CondicionPago { get; set; } = null!;

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;
}

public class FacturaAdminDetalleViewModel
{
    public int Id { get; set; }

    public string NumeroFactura { get; set; } = null!;

    public string? NumeroCotizacion { get; set; }

    public string NombreCliente { get; set; } = null!;

    public string CorreoCliente { get; set; } = null!;

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public string CondicionPago { get; set; } = null!;

    public string? ComprobanteSinpeRuta { get; set; }

    public string Estado { get; set; } = null!;

    public List<DetalleFacturarDto> Lineas { get; set; } = new();

    public decimal Subtotal { get; set; }

    public decimal DescuentoTotal { get; set; }

    public decimal ImpuestoTotal { get; set; }

    public decimal Total { get; set; }
}

public class FacturacionIndexViewModel
{
    public List<FacturaAdminListadoDto> Facturas { get; set; } = new();

    public List<CotizacionListadoDto> CotizacionesFacturables { get; set; } = new();
}
