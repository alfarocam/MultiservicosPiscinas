using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models
{
    public class AnuncioCreateViewModel
    {
        [Required(ErrorMessage = "El título es obligatorio.")]
        [StringLength(200, ErrorMessage = "El título no puede superar los 200 caracteres.")]
        [Display(Name = "Título")]
        public string Titulo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La descripción es obligatoria.")]
        [Display(Name = "Descripción")]
        public string Contenido { get; set; } = string.Empty;

        [Display(Name = "Urgente")]
        public bool Urgente { get; set; }
    }
}