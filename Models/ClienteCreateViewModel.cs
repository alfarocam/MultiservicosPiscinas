using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models
{
    public class ClienteCreateViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(9, ErrorMessage = "El teléfono debe tener el formato 8888-8888.")]
        [RegularExpression(@"^\d{4}-?\d{4}$", ErrorMessage = "El teléfono debe contener 8 dígitos con el formato 8888-8888.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
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

    public class ClienteEditViewModel
    {
        public int Id { get; set; }

        public int UsuarioId { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        [StringLength(9, ErrorMessage = "El teléfono debe tener el formato 8888-8888.")]
        [RegularExpression(@"^\d{4}-?\d{4}$", ErrorMessage = "El teléfono debe contener 8 dígitos con el formato 8888-8888.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un distrito.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un distrito válido.")]
        public int DistritoId { get; set; }

        [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
        public double? Latitud { get; set; }

        [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
        public double? Longitud { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden superar los 500 caracteres.")]
        public string? Notas { get; set; }
    }
}