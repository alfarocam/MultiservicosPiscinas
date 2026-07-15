using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models;

public class MiAgendaViewModel
{
    public DateOnly FechaConsulta { get; set; }

    public List<MiAgendaCitaViewModel> Citas { get; set; } = new();
}

public class MiAgendaCitaViewModel
{
    public int CitaId { get; set; }

    public int? ServicioId { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Piscina { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public string TipoServicio { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; }

    public bool PuedeReprogramar { get; set; }
}

public class ReprogramarCitaTecnicoViewModel
{
    public int CitaId { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Piscina { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public string TipoServicio { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public DateTime FechaHoraActual { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una nueva fecha.")]
    [Display(Name = "Nueva fecha")]
    public DateOnly NuevaFecha { get; set; }

    [Required(ErrorMessage = "Debe seleccionar una nueva hora.")]
    [Display(Name = "Nueva hora")]
    public TimeOnly NuevaHora { get; set; }

    [StringLength(500, ErrorMessage = "El motivo no puede superar los 500 caracteres.")]
    [Display(Name = "Motivo de reprogramación")]
    public string? Motivo { get; set; }
}