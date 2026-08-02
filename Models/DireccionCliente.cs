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

    // HU-12.1: coordenadas para optimización de rutas 
    // Nullable porque las direcciones ya existentes en la base de datos
    // no tienen coordenadas todavía.
    public double? Latitud { get; set; }

    public double? Longitud { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;

    public virtual Distrito Distrito { get; set; } = null!;

    public virtual ICollection<Piscina> Piscina { get; set; } = new List<Piscina>();
}