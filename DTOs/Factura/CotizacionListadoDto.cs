namespace MultiserviciosPiscinas.DTOs.Factura;

public class CotizacionListadoDto
{
    public int Id { get; set; }

    public string NumeroCotizacion { get; set; } = null!;

    public string NombreCliente { get; set; } = null!;

    public DateOnly FechaEmision { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;
}
