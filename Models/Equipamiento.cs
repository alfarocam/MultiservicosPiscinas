using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Equipamiento
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public virtual ICollection<PiscinaEquipamiento> PiscinaEquipamiento { get; set; } = new List<PiscinaEquipamiento>();
}
