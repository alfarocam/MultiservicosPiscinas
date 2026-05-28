using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Piscina
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public int DireccionId { get; set; }

    public string Tipo { get; set; } = null!;

    public double VolumenM3 { get; set; }

    public string Estado { get; set; } = null!;

    public virtual ICollection<Citum> Cita { get; set; } = new List<Citum>();

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual DireccionCliente Direccion { get; set; } = null!;

    public virtual ICollection<PiscinaEquipamiento> PiscinaEquipamientos { get; set; } = new List<PiscinaEquipamiento>();

    public virtual ICollection<ProyectoConstruccion> ProyectoConstruccions { get; set; } = new List<ProyectoConstruccion>();
}
