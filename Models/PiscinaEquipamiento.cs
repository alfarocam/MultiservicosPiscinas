using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class PiscinaEquipamiento
{
    public int Id { get; set; }

    public int PiscinaId { get; set; }

    public int EquipamientoId { get; set; }

    public virtual Equipamiento Equipamiento { get; set; } = null!;

    public virtual Piscina Piscina { get; set; } = null!;
}
