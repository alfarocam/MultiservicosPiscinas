namespace MultiserviciosPiscinas.DTOs.Cotizacion
{
    public class DetalleCotizacionClienteDto
    {
        public int ProductoId { get; set; }
        public string NombreProducto { get; set; } = null!;
        public string? DescripcionProducto { get; set; }
        public decimal Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Impuesto { get; set; }
        public decimal LineaTotal { get; set; }
    }
}
