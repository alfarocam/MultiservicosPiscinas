using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class CategoriaGastoOperativo
{
    public int Id { get; set; }

    public string NombreCategoria { get; set; } = null!;

    public virtual ICollection<GastoOperativo> GastoOperativos { get; set; } = new List<GastoOperativo>();
}
