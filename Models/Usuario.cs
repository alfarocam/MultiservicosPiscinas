using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class Usuario
{
    public int Id { get; set; }

    public int RolId { get; set; }

    public string Nombre { get; set; } = null!;

    public string ApellidoPaterno { get; set; } = null!;

    public string ApellidoMaterno { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string Contrasena { get; set; } = null!;

    public bool Activo { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual ICollection<Anuncio> Anuncio { get; set; } = new List<Anuncio>();

    public virtual ICollection<BitacoraAuditoria> BitacoraAuditoria { get; set; } = new List<BitacoraAuditoria>();

    public virtual ICollection<Cita> Cita { get; set; } = new List<Cita>();

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<GastoOperativo> GastoOperativo { get; set; } = new List<GastoOperativo>();

    public virtual Rol Rol { get; set; } = null!;

    public virtual ICollection<RutaOptimizada> RutaOptimizada { get; set; } = new List<RutaOptimizada>();

    public virtual ICollection<Vehiculo> Vehiculo { get; set; } = new List<Vehiculo>();
}
