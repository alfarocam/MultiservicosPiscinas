using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class RutaOptimizada
{
    public int Id { get; set; }

    public int TecnicoId { get; set; }

    public DateOnly Fecha { get; set; }

    public double? DistanciaTotalKm { get; set; }

    public int? DuracionTotalMin { get; set; }

    public DateTime GeneradaEn { get; set; }

    public string? EnlaceGoogleMaps { get; set; }

    public virtual Usuario Tecnico { get; set; } = null!;

    public virtual ICollection<VisitaRuta> VisitaRuta { get; set; } = new List<VisitaRuta>();
}
