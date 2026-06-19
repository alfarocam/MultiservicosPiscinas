using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models
{
    public class ProyectoCreateViewModel
    {
        [Required(ErrorMessage = "Debe seleccionar un cliente.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un cliente válido.")]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [Display(Name = "Piscina asociada")]
        public int? PiscinaId { get; set; }

        [Required(ErrorMessage = "El nombre del proyecto es obligatorio.")]
        [StringLength(150)]
        [Display(Name = "Nombre del proyecto")]
        public string Nombre { get; set; } = string.Empty;

        [Display(Name = "Descripción")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
        [Display(Name = "Fecha de inicio")]
        public DateOnly FechaInicio { get; set; }

        [Display(Name = "Fecha de fin estimada")]
        public DateOnly? FechaFinEstimada { get; set; }

        [Required(ErrorMessage = "Debe seleccionar un estado.")]
        [Display(Name = "Estado")]
        public string Estado { get; set; } = string.Empty;

        [Required(ErrorMessage = "El presupuesto es obligatorio.")]
        [Range(0.01, 999999999.99, ErrorMessage = "El presupuesto debe ser mayor a cero.")]
        [Display(Name = "Presupuesto (₡)")]
        public decimal Presupuesto { get; set; }
    }
}
