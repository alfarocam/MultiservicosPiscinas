using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public partial class BitacoraAuditorium
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public string Accion { get; set; } = null!;

    public string TablaAfectada { get; set; } = null!;

    public int RegistroId { get; set; }

    public string? ValorAnterior { get; set; }

    public string ValorNuevo { get; set; } = null!;

    public DateTime FechaHora { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;
}
