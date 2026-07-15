using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class TareaServicio
{
    public int Id { get; set; }

    public int ServicioId { get; set; }

    public string Descripcion { get; set; } = null!;

    public string Estado { get; set; } = null!;

    public DateOnly FechaAsignacion { get; set; }

    public DateOnly? FechaCompletacion { get; set; }

    public virtual Servicio Servicio { get; set; } = null!;
}
