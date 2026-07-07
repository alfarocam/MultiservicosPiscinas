namespace MultiserviciosPiscinas.Models;

public partial class Notificacion
{
    public int Id { get; set; }

    public int UsuarioId { get; set; }

    public int? CitaId { get; set; }

    public string Mensaje { get; set; } = null!;

    public bool Leida { get; set; }

    public DateTime FechaCreacion { get; set; }

    public virtual Usuario Usuario { get; set; } = null!;

    public virtual Cita? Cita { get; set; }
}