using System.ComponentModel.DataAnnotations;

namespace MultiservicosPiscinas.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        [StringLength(100)]
        [Display(Name = "Nombre completo")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        [StringLength(150)]
        [Display(Name = "Correo electrónico")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [StringLength(20)]
        [Display(Name = "Teléfono")]
        public string Telefono { get; set; } = string.Empty;

        [StringLength(200)]
        [Display(Name = "Dirección")]
        public string Direccion { get; set; } = string.Empty;

        [Display(Name = "Fecha de registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;

        [Display(Name = "Activo")]
        public bool Activo { get; set; } = true;

        // Relación: un cliente puede tener varias piscinas
        public ICollection<Piscina> Piscinas { get; set; } = new List<Piscina>();
    }
}