using System.ComponentModel.DataAnnotations;
using MultiserviciosPiscinas.DTOs.Cotizacion;

namespace MultiserviciosPiscinas.DTOs.Factura;

public class ComprarCarritoViewModel
{
    public string NombreCliente { get; set; } = null!;

    public DateOnly FechaEmision { get; set; }

    public List<ItemCarritoDto> Lineas { get; set; } = new();

    public decimal Subtotal { get; set; }

    public decimal ImpuestoTotal { get; set; }

    public decimal Total { get; set; }

    [Required(ErrorMessage = "Debe subir el comprobante de pago.")]
    public IFormFile? Comprobante { get; set; }
}
