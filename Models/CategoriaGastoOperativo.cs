using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class CategoriaGastoOperativo
{
    public int Id { get; set; }

    public string NombreCategoria { get; set; } = null!;

    public virtual ICollection<GastoOperativo> GastoOperativo { get; set; } = new List<GastoOperativo>();
}
