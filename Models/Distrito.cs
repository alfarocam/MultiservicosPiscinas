using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Distrito
{
    public int Id { get; set; }

    public int CantonId { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual Canton Canton { get; set; } = null!;

    public virtual ICollection<DireccionCliente> DireccionClientes { get; set; } = new List<DireccionCliente>();
}
