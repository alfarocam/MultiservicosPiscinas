using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models
{
    public class CitaCreateViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido.")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una piscina.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una piscina válida.")]
        [Display(Name = "Piscina")]
        public int PiscinaId { get; set; }

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

        [Required(ErrorMessage = "Debe indicar el tipo de servicio.")]
        [StringLength(50)]
        [Display(Name = "Tipo de servicio")]
        public string Tipo { get; set; } = string.Empty;

        [StringLength(500)]
        [Display(Name = "Notas")]
        public string? Notas { get; set; }
    }
}