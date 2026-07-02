namespace MultiserviciosPiscinas.DTOs.Factura;

public class DetalleFacturarDto
{
    public int ProductoId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Cantidad { get; set; }

    public decimal LineaImpuesto { get; set; }

    public decimal LineaSubtotal => PrecioUnitario * Cantidad;

    public decimal LineaTotal => LineaSubtotal + LineaImpuesto;
}
