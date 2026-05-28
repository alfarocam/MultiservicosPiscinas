using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiservicosPiscinas.Models;

public partial class Cliente
{
    [NotMapped]
    public string Nombre { get; set; } = string.Empty;

    [NotMapped]
    public string Correo { get; set; } = string.Empty;

    [NotMapped]
    public string Telefono { get; set; } = string.Empty;

    [NotMapped]
    public string Direccion { get; set; } = string.Empty;

    [NotMapped]
    public DateTime FechaRegistro { get; set; } = DateTime.Now;

    [NotMapped]
    public bool Activo { get; set; } = true;
}
