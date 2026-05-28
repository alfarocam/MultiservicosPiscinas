using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class Encuestum
{
    public int Id { get; set; }

    public int ServicioId { get; set; }

    public int Calificacion { get; set; }

    public string? Comentario { get; set; }

    public DateTime FechaEnvio { get; set; }

    public virtual Servicio Servicio { get; set; } = null!;
}
