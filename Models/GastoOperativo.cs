using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class GastoOperativo
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int? CitaId { get; set; }

    public int CategoriaId { get; set; }

    public decimal Monto { get; set; }

    public DateOnly Fecha { get; set; }

    public string? Descripcion { get; set; }

    public string Estado { get; set; } = null!;

    public string? MotivoRechazo { get; set; }

    public string? ComprobanteRuta { get; set; }

    public virtual CategoriaGastoOperativo Categoria { get; set; } = null!;

    public virtual Cita? Cita { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
