using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class Producto
{
    public int Id { get; set; }

    public int CategoriaId { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public bool Activo { get; set; }

    public virtual CategoriaProducto Categoria { get; set; } = null!;

    public virtual ICollection<DetalleCotizacion> DetalleCotizacions { get; set; } = new List<DetalleCotizacion>();

    public virtual ICollection<DetalleFactura> DetalleFacturas { get; set; } = new List<DetalleFactura>();

    public virtual ICollection<ItemCarrito> ItemCarritos { get; set; } = new List<ItemCarrito>();
}
