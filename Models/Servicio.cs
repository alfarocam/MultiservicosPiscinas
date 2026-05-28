using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class Servicio
{
    public int Id { get; set; }

    public int CitaId { get; set; }

    public DateOnly FechaApertura { get; set; }

    public DateOnly? FechaCierre { get; set; }

    public string Estado { get; set; } = null!;

    public string TrabajoRealizado { get; set; } = null!;

    public virtual Citum Cita { get; set; } = null!;

    public virtual Encuestum? Encuestum { get; set; }

    public virtual ICollection<Inspeccion> Inspeccions { get; set; } = new List<Inspeccion>();

    public virtual ICollection<TareaServicio> TareaServicios { get; set; } = new List<TareaServicio>();
}
