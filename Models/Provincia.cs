using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Provincia
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Canton> Canton { get; set; } = new List<Canton>();
}
