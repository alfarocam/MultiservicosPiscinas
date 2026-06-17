using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class DireccionCliente
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public int DistritoId { get; set; }

    public string TipoDireccion { get; set; } = null!;

    public string Detalles { get; set; } = null!;

    public int? CodigoPostal { get; set; }

    public byte EsPrincipal { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Distrito Distrito { get; set; } = null!;

    public virtual ICollection<Piscina> Piscina { get; set; } = new List<Piscina>();
}
