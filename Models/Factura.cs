using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class Factura
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public int? CotizacionId { get; set; }

    public int CreadoPor { get; set; }

    public string NumeroConsecutivo { get; set; } = null!;

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVencimiento { get; set; }

    public string CondicionPago { get; set; } = null!;

    public string? ComprobanteSinpeRuta { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DescuentoTotal { get; set; }

    public decimal ImpuestoTotal { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Cotizacion? Cotizacion { get; set; }

    public virtual ICollection<DetalleFactura> DetalleFacturas { get; set; } = new List<DetalleFactura>();
}
