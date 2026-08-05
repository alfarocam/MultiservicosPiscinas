using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models
{
    // Modelo del formulario público de registro (Auth/Registrar). Se separa de
    // la entidad Usuario (a diferencia del código anterior, que bindeaba el POST
    // directo contra Usuario) para no depender de sus propiedades de navegación
    // y para poder pedir aquí los mismos datos de contacto/dirección que ya se
    // le piden al cliente cuando lo registra el administrador (ClienteCreateViewModel),
    // más la contraseña que en ese flujo no aplica porque el admin no la define.
    public class UsuarioRegistroViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio.")]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido paterno es obligatorio.")]
        public string ApellidoPaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El apellido materno es obligatorio.")]
        public string ApellidoMaterno { get; set; } = string.Empty;

        [Required(ErrorMessage = "El correo es obligatorio.")]
        [EmailAddress(ErrorMessage = "Ingresá un correo válido.")]
        public string Correo { get; set; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        [DataType(DataType.Password)]
        public string Contrasena { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio.")]
        public string Telefono { get; set; } = string.Empty;

        [Required(ErrorMessage = "La dirección es obligatoria.")]
        public string Direccion { get; set; } = string.Empty;

        [Required(ErrorMessage = "Debe seleccionar un distrito.")]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar un distrito válido.")]
        public int DistritoId { get; set; }

        // Igual que en ClienteCreateViewModel: opcionales, porque sin el pin en
        // el mapa no hay forma de saberlas, pero no deben bloquear la creación
        // de la cuenta (la dirección simplemente no entrará en rutas optimizadas
        // hasta que se complete).
        [Range(-90, 90, ErrorMessage = "La latitud debe estar entre -90 y 90.")]
        public double? Latitud { get; set; }

        [Range(-180, 180, ErrorMessage = "La longitud debe estar entre -180 y 180.")]
        public double? Longitud { get; set; }
    }
}
