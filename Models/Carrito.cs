using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Carrito
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public DateTime CreadoEn { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual ICollection<ItemCarrito> ItemCarritos { get; set; } = new List<ItemCarrito>();
}
