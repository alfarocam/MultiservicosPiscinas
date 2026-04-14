using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MultiservicosPiscinas.Models
{
    public class Piscina
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre / identificador")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Tipo")]
        public string Tipo { get; set; } = string.Empty; // Residencial, Comercial, etc.

        [Display(Name = "Largo (m)")]
        public double Largo { get; set; }

        [Display(Name = "Ancho (m)")]
        public double Ancho { get; set; }

        [Display(Name = "Profundidad (m)")]
        public double Profundidad { get; set; }

        [StringLength(300)]
        [Display(Name = "Observaciones")]
        public string Observaciones { get; set; } = string.Empty;

        [Display(Name = "Activa")]
        public bool Activa { get; set; } = true;

        // Llave foránea al cliente
        [Required]
        [Display(Name = "Cliente")]
        public int ClienteId { get; set; }

        [ForeignKey("ClienteId")]
        public Cliente? Cliente { get; set; }
    }
}