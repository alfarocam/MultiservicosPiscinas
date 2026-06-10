namespace MultiserviciosPiscinas.Models
{
    public partial class ContactoEmpresa
    {

        public string Telefono { get; set; } = null!;

        public string Correo { get; set; } = null!;

        public string Direccion { get; set; } = null!;

        public string? HorarioAtencion { get; set; }
    }
}