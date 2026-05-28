using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Inspeccion
{
    public int Id { get; set; }

    public int ServicioId { get; set; }

    public DateOnly FechaInspeccion { get; set; }

    public double? CloroPpm { get; set; }

    public double? Alcalinidad { get; set; }

    public double? Ph { get; set; }

    public double? Calcio { get; set; }

    public double? AcidoCianurico { get; set; }

    public string? Observaciones { get; set; }

    public virtual Servicio Servicio { get; set; } = null!;
}
