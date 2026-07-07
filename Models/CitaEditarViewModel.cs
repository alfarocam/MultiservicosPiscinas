using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models
{
    public class CitaEditarViewModel
    {
        public int Id { get; set; }

        public string ClienteNombre { get; set; } = string.Empty;

        public string PiscinaDescripcion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un técnico.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un técnico válido.")]
        [Display(Name = "Técnico asignado")]
        public int TecnicoId { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [Display(Name = "Fecha")]
        public DateOnly Fecha { get; set; }

        [Required(ErrorMessage = "La hora es obligatoria.")]
        [Display(Name = "Hora")]
        public TimeOnly Hora { get; set; }

        public string Estado { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Notas")]
        public string? Notas { get; set; }
    }
}