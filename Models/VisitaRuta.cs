using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class VisitaRuta
{
    public int Id { get; set; }

    public int RutaId { get; set; }

    public int CitaId { get; set; }

    public int OrdenVisita { get; set; }

    public double? DistanciaTramoKm { get; set; }

    public int? DuracionTramoMin { get; set; }

    public virtual Cita Cita { get; set; } = null!;

    public virtual RutaOptimizada Ruta { get; set; } = null!;
}
