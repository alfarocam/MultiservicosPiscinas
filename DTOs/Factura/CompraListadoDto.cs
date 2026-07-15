namespace MultiserviciosPiscinas.DTOs.Factura;

public class CompraListadoDto
{
    public int FacturaId { get; set; }

    public string NumeroFactura { get; set; } = null!;

    public DateOnly FechaEmision { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;
}
