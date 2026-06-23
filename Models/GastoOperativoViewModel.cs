using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models
{
    public class GastoOperativoViewModel
    {
        [Required(ErrorMessage = "La categoría es obligatoria.")]
        [Display(Name = "Categoría")]
        public int CategoriaId { get; set; }

        [Required(ErrorMessage = "El monto es obligatorio.")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a cero.")]
        [Display(Name = "Monto (₡)")]
        public decimal Monto { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria.")]
        [Display(Name = "Fecha")]
        public DateOnly Fecha { get; set; }

        [Display(Name = "Descripción")]
        [MaxLength(500, ErrorMessage = "La descripción no puede superar los 500 caracteres.")]
        public string? Descripcion { get; set; }

        [Display(Name = "Comprobante")]
        public IFormFile? Comprobante { get; set; }
    }
}