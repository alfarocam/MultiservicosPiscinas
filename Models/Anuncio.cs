using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class Anuncio
{
    public int Id { get; set; }

    public int AutorId { get; set; }

    public string Titulo { get; set; } = null!;

    public string Contenido { get; set; } = null!;

    public string Prioridad { get; set; } = null!;

    public DateOnly FechaPublicacion { get; set; }

    public DateOnly? FechaCaducidad { get; set; }

    public virtual Usuario Autor { get; set; } = null!;
}
