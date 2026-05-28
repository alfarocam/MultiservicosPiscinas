using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class VisitaRutum
{
    public int Id { get; set; }

    public int RutaId { get; set; }

    public int CitaId { get; set; }

    public int OrdenVisita { get; set; }

    public double? DistanciaTramoKm { get; set; }

    public int? DuracionTramoMin { get; set; }

    public virtual Citum Cita { get; set; } = null!;

    public virtual RutaOptimizadum Ruta { get; set; } = null!;
}
