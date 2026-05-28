using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class ProyectoConstruccion
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public int? PiscinaId { get; set; }

    public string? Nombre { get; set; }

    public string? Descripcion { get; set; }

    public DateOnly FechaInicio { get; set; }

    public DateOnly? FechaFinEstimada { get; set; }

    public string? Estado { get; set; }

    public decimal Presupuesto { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Piscina? Piscina { get; set; }
}
