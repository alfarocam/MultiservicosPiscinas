using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class DetalleCotizacion
{
    public int Id { get; set; }

    public int CotizacionId { get; set; }

    public int ProductoId { get; set; }

    public decimal CantidadPropuesta { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuesto { get; set; }

    public decimal LineaSubtotal { get; set; }

    public decimal LineaTotal { get; set; }

    public virtual Cotizacion Cotizacion { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}
