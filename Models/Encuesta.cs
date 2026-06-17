using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Encuesta
{
    public int Id { get; set; }

    public int ServicioId { get; set; }

    public int Calificacion { get; set; }

    public string? Comentario { get; set; }

    public DateTime FechaEnvio { get; set; }

    public virtual Servicio Servicio { get; set; } = null!;
}
