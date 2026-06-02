namespace MultiserviciosPiscinas.Models
{
    public class PerfilViewModel
    {
        public string NombreCompleto { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Telefono { get; set; } = string.Empty;

        public string Rol { get; set; } = string.Empty;

        public List<string> Permisos { get; set; } = new();
    }
}