using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class Cotizacion
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public DateOnly FechaEmision { get; set; }

    public DateOnly FechaVigencia { get; set; }

    public decimal Subtotal { get; set; }

    public decimal DescuentoTotal { get; set; }

    public decimal ImpuestoTotal { get; set; }

    public decimal Total { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<DetalleCotizacion> DetalleCotizacions { get; set; } = new List<DetalleCotizacion>();

    public virtual Factura? Factura { get; set; }
}
