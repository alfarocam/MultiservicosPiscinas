using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Provincium
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Canton> Cantons { get; set; } = new List<Canton>();
}
