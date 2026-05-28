using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class DetalleFactura
{
    public int Id { get; set; }

    public int FacturaId { get; set; }

    public int ProductoId { get; set; }

    public decimal CantidadVendida { get; set; }

    public decimal PrecioUnitarioFinal { get; set; }

    public decimal Descuento { get; set; }

    public decimal Impuesto { get; set; }

    public decimal LineaSubtotal { get; set; }

    public decimal LineaTotal { get; set; }

    public virtual Factura Factura { get; set; } = null!;

    public virtual Producto Producto { get; set; } = null!;
}
