using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Canton
{
    public int Id { get; set; }

    public int ProvinciaId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<Distrito> Distrito { get; set; } = new List<Distrito>();

    public virtual Provincia Provincia { get; set; } = null!;
}
