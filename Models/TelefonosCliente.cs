using System;
using System.Collections.Generic;

namespace MultiservicosPiscinas.Models;

public partial class TelefonosCliente
{
    public int Id { get; set; }

    public int ClienteId { get; set; }

    public string TipoTelefono { get; set; } = null!;

    public string NumeroTelefono { get; set; } = null!;

    public byte EsPrincipal { get; set; }

    public virtual Cliente Cliente { get; set; } = null!;
}
