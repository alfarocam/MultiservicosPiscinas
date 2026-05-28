using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class Vehiculo
{
    public int Id { get; set; }

    public int TecnicoId { get; set; }

    public string Placa { get; set; } = null!;

    public string Marca { get; set; } = null!;

    public string? Modelo { get; set; }

    public string Estado { get; set; } = null!;

    public virtual Usuario Tecnico { get; set; } = null!;
}
