namespace MultiserviciosPiscinas.Models
{
    public partial class ConsultaContacto
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public string Asunto { get; set; } = string.Empty;

        public string Mensaje { get; set; } = string.Empty;

        public DateTime FechaEnvio { get; set; }
    }
}