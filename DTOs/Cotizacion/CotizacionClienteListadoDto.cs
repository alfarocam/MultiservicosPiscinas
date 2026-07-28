namespace MultiserviciosPiscinas.DTOs.Cotizacion
{
    public class CotizacionClienteListadoDto
    {
        public int Id { get; set; }
        public string NumeroCotizacion { get; set; } = null!;
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVigencia { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = null!;
    }
}
