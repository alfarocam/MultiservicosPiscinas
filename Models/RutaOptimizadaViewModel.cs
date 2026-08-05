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

        // Antes la vista solo miraba TieneSuficientesParadas (>= 2 citas ese día),
        // sin importar si esas citas tenían coordenadas. Si el técnico tenía 2+
        // citas pero ninguna con lat/lng (por ejemplo, direcciones de clientes
        // creadas antes del selector de mapa), la vista igual entraba a la rama
        // que arma el mapa, y como el JS filtra por TieneCoordenadas terminaba
        // con menos de 2 paradas: el mapa de Google nunca se inicializaba y la
        // pantalla se quedaba en blanco sin ningún mensaje.
        public bool TieneSuficientesParadasConCoordenadas => ParadasConCoordenadas >= 2;

        public string GoogleMapsApiKey { get; set; } = string.Empty;
    }
}