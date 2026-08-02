using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models
{
    public class ClienteCreateViewModel
    {
        [Required]
        public string Nombre { get; set; } = string.Empty;

        [Required]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Required]
        public string ApellidoMaterno { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un distrito.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un distrito válido.")]
        public int DistritoId { get; set; }

        // --- HU-12.1: coordenadas para optimización de rutas ---
        // Opcionales por ahora: si el administrador no las conoce de memoria,
        // puede completarlas luego editando la dirección. Sin ellas, esta
        // dirección simplemente no podrá incluirse en una ruta optimizada.
        [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
        public double? Latitud { get; set; }

        [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
        public double? Longitud { get; set; }
    }
}