using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiservicosPiscinas.Models;

public partial class Piscina
{
    [NotMapped]
    public string Nombre { get; set; } = string.Empty;

    [NotMapped]
    public double Largo { get; set; }

    [NotMapped]
    public double Ancho { get; set; }

    [NotMapped]
    public double Profundidad { get; set; }

    [NotMapped]
    public bool Activa { get; set; } = true;

    [NotMapped]
    public string Observaciones { get; set; } = string.Empty;
}
