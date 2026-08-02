namespace MultiserviciosPiscinas.Models
{
    // Una parada individual de la ruta (una cita/piscina a visitar).
    public class ParadaRutaViewModel
    {
        public int CitaId { get; set; }

        public string ClienteNombre { get; set; } = string.Empty;

        public string Direccion { get; set; } = string.Empty;

        public double? Latitud { get; set; }

        public double? Longitud { get; set; }

        public DateTime HoraCita { get; set; }

        public string TipoServicio { get; set; } = string.Empty;

        // Una parada solo es utilizable por Google Maps si tiene coordenadas.
        public bool TieneCoordenadas => Latitud.HasValue && Longitud.HasValue;
    }

    // Datos completos que necesita la vista Index de RutaOptimizada.
    public class RutaOptimizadaViewModel
    {
        public int TecnicoId { get; set; }

        public string TecnicoNombre { get; set; } = string.Empty;

        public DateOnly Fecha { get; set; }

        public List<ParadaRutaViewModel> Paradas { get; set; } = new();

        // Escenario 2 de HU-12.1: se necesitan al menos 2 destinos para optimizar.
        public bool TieneSuficientesParadas => Paradas.Count >= 2;

        // Cuántas de las paradas realmente tienen coordenadas cargadas
        // (una dirección sin lat/lng no puede incluirse en la ruta de Google Maps).
        public int ParadasConCoordenadas => Paradas.Count(p => p.TieneCoordenadas);

        // Necesaria para inicializar el mapa en la vista sin exponer configuración
        // directamente desde appsettings/secrets en el .cshtml.
        public string GoogleMapsApiKey { get; set; } = string.Empty;
    }
}