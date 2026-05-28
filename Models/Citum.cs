using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Citum
{
    public int Id { get; set; }

    public int PiscinaId { get; set; }

    public int TecnicoId { get; set; }

    public DateTime FechaHora { get; set; }

    public string Tipo { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public string? Notas { get; set; }

    public virtual ICollection<GastoOperativo> GastoOperativos { get; set; } = new List<GastoOperativo>();

    public virtual Piscina Piscina { get; set; } = null!;

    public virtual Servicio? Servicio { get; set; }

    public virtual Usuario Tecnico { get; set; } = null!;

    public virtual VisitaRutum? VisitaRutum { get; set; }
}
