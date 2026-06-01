namespace MultiserviciosPiscinas.DTOs
{
    public class HistorialServicioDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string EstadoCita { get; set; } = string.Empty;
        public DateOnly FechaApertura { get; set; }
        public DateOnly? FechaCierre { get; set; }
        public string EstadoServicio { get; set; } = string.Empty;
        public string TrabajoRealizado { get; set; } = string.Empty;
        public string NombreTecnico { get; set; } = string.Empty;
    }
}
