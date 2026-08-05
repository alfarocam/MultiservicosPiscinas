using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models;

public class ServicioCreateViewModel
{
    [Required(ErrorMessage = "Debe seleccionar una cita.")]
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una cita válida.")]
    [Display(Name = "Cita")]
    public int CitaId { get; set; }

    [Required(ErrorMessage = "Debe indicar el trabajo a realizar.")]
    [Display(Name = "Trabajo a realizar")]
    public string TrabajoRealizado { get; set; } = string.Empty;
}
