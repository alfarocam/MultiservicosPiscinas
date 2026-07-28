using System.Collections.Generic;

namespace MultiserviciosPiscinas.DTOs.Cotizacion
{
    public class CotizacionClienteDetalleDto
    {
        public int Id { get; set; }
        public string NumeroCotizacion { get; set; } = null!;
        public DateOnly FechaEmision { get; set; }
        public DateOnly FechaVigencia { get; set; }
        public decimal Subtotal { get; set; }
        public decimal ImpuestoTotal { get; set; }
        public decimal Total { get; set; }
        public string Estado { get; set; } = null!;
        
        public List<DetalleCotizacionClienteDto> Lineas { get; set; } = new List<DetalleCotizacionClienteDto>();
    }
}
