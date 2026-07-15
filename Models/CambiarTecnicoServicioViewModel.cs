using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models;

public class CambiarTecnicoServicioViewModel
{
    public int ServicioId { get; set; }
    public int CitaId { get; set; }

    public string Cliente { get; set; } = string.Empty;
    public string Piscina { get; set; } = string.Empty;
    public string TecnicoActual { get; set; } = string.Empty;
    public DateTime FechaHoraCita { get; set; }
    public string EstadoServicio { get; set; } = string.Empty;

    [Required(ErrorMessage = "Debe seleccionar un técnico.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un técnico válido.")]
    [Display(Name = "Nuevo técnico asignado")]
    public int TecnicoId { get; set; }
}
