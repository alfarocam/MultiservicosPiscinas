namespace MultiserviciosPiscinas.DTOs
{
    public class DetalleServicioDto
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string EstadoCita { get; set; } = string.Empty;
        public string Notas { get; set; } = string.Empty;
        public DateOnly FechaApertura { get; set; }
        public DateOnly? FechaCierre { get; set; }
        public string EstadoServicio { get; set; } = string.Empty;
        public string TrabajoRealizado { get; set; } = string.Empty;
        public string NombreTecnico { get; set; } = string.Empty;
        public List<TareaDto> Tareas { get; set; } = new();
        public InspeccionDto? Inspeccion { get; set; }
    }

    public class TareaDto
    {
        public string Descripcion { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateOnly FechaAsignacion { get; set; }
        public DateOnly? FechaCompletacion { get; set; }
    }

    public class InspeccionDto
    {
        public DateOnly FechaInspeccion { get; set; }
        public double? CloroPpm { get; set; }
        public double? Alcalinidad { get; set; }
        public double? Ph { get; set; }
        public double? Calcio { get; set; }
        public double? AcidoCianurico { get; set; }
        public string? Observaciones { get; set; }
    }
}
