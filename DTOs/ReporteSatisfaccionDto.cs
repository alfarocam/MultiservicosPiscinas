namespace MultiserviciosPiscinas.DTOs
{
    public class ReporteSatisfaccionDto
    {
        public double PromedioGeneral { get; set; }
        public int TotalEncuestas { get; set; }
        public int TotalCriticas { get; set; }    // calificacion <= 2
        public List<PromedioTecnicoDto> PromediosPorTecnico { get; set; } = new();
        public List<EncuestaDetalleDto> UltimasEncuestas { get; set; } = new();
        public DateTime? FechaDesde { get; set; }
        public DateTime? FechaHasta { get; set; }
    }

    public class PromedioTecnicoDto
    {
        public string NombreTecnico { get; set; } = string.Empty;
        public double Promedio { get; set; }
        public int TotalEncuestas { get; set; }
    }

    public class EncuestaDetalleDto
    {
        public int Id { get; set; }
        public int ServicioId { get; set; }
        public string TipoServicio { get; set; } = string.Empty;
        public string NombreTecnico { get; set; } = string.Empty;
        public int Calificacion { get; set; }
        public string? Comentario { get; set; }
        public DateTime FechaEnvio { get; set; }
    }
}
