using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

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

    public virtual ICollection<Anuncio> Anuncios { get; set; } = new List<Anuncio>();

    public virtual ICollection<BitacoraAuditorium> BitacoraAuditoria { get; set; } = new List<BitacoraAuditorium>();

    public virtual ICollection<Citum> Cita { get; set; } = new List<Citum>();

    public virtual Cliente? Cliente { get; set; }

    public virtual ICollection<GastoOperativo> GastoOperativos { get; set; } = new List<GastoOperativo>();

    public virtual Rol Rol { get; set; } = null!;

    public virtual ICollection<RutaOptimizadum> RutaOptimizada { get; set; } = new List<RutaOptimizadum>();

    public virtual ICollection<Vehiculo> Vehiculos { get; set; } = new List<Vehiculo>();
}
