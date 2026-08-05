namespace MultiserviciosPiscinas.Models
{
    public class ParadaRutaViewModel
    {
        public int VisitaRutaId { get; set; }

        public int CitaId { get; set; }

        public int OrdenVisita { get; set; }

        public string ClienteNombre { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public double? Latitud { get; set; }

        public double? Longitud { get; set; }

        public DateTime HoraCita { get; set; }

        public string TipoServicio { get; set; } = string.Empty;

        public string EstadoCita { get; set; } = string.Empty;

        public double? DistanciaTramoKm { get; set; }

        public int? DuracionTramoMin { get; set; }

        public bool TieneCoordenadas => Latitud.HasValue && Longitud.HasValue;
    }

    public class RutaOptimizadaViewModel
    {
        public int RutaId { get; set; }

        public int TecnicoId { get; set; }

        public string TecnicoNombre { get; set; } = string.Empty;

        public DateOnly Fecha { get; set; }

        public double? DistanciaTotalKm { get; set; }

        public int? DuracionTotalMin { get; set; }

        public string? EnlaceGoogleMaps { get; set; }

        public List<ParadaRutaViewModel> Paradas { get; set; } = new();

        public bool TieneRutaOptimizada => RutaId > 0;

        public bool TieneSuficientesParadas => Paradas.Count >= 2;

        public int ParadasConCoordenadas => Paradas.Count(p => p.TieneCoordenadas);

        public string GoogleMapsApiKey { get; set; } = string.Empty;
    }
}