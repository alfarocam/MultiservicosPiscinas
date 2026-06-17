using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Servicio
{
    public int Id { get; set; }

    public int CitaId { get; set; }

    public DateOnly FechaApertura { get; set; }

    public DateOnly? FechaCierre { get; set; }

    public string Estado { get; set; } = null!;

    public string TrabajoRealizado { get; set; } = null!;

    public virtual Cita Cita { get; set; } = null!;

    public virtual Encuesta? Encuesta { get; set; }

    public virtual ICollection<Inspeccion> Inspeccion { get; set; } = new List<Inspeccion>();

    public virtual ICollection<TareaServicio> TareaServicio { get; set; } = new List<TareaServicio>();
}
