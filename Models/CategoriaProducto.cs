using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class CategoriaProducto
{
    public int Id { get; set; }

    public string NombreCategoria { get; set; } = null!;

    public virtual ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
