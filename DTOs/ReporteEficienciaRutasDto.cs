namespace MultiserviciosPiscinas.DTOs
{
    public class ReporteEficienciaRutasDto
    {
        public DateTime? FechaDesde { get; set; }

        public DateTime? FechaHasta { get; set; }

        public List<EficienciaRutaTecnicoDto> Tecnicos { get; set; } = new();

        public List<EficienciaRutaDetalleDto> Detalle { get; set; } = new();

        public double TotalDistanciaOptimizadaKm => Tecnicos.Sum(t => t.DistanciaOptimizadaKm);

        public double TotalDistanciaRealKm => Tecnicos.Sum(t => t.DistanciaRealKm);

        public int TotalRutas => Tecnicos.Sum(t => t.TotalRutas);

        public int TotalVisitas => Tecnicos.Sum(t => t.TotalVisitas);

        public double DiferenciaTotalKm => TotalDistanciaRealKm - TotalDistanciaOptimizadaKm;

        public double EficienciaGeneralPorcentaje
        {
            get
            {
                if (TotalDistanciaRealKm <= 0)
                {
                    return 0;
                }

                return (TotalDistanciaOptimizadaKm / TotalDistanciaRealKm) * 100;
            }
        }
    }

    public class EficienciaRutaTecnicoDto
    {
        public int TecnicoId { get; set; }

        public string Tecnico { get; set; } = string.Empty;

        public int TotalRutas { get; set; }

        public int TotalVisitas { get; set; }

        public int VisitasCompletadas { get; set; }

        public double DistanciaOptimizadaKm { get; set; }

        public double DistanciaRealKm { get; set; }

        public int DuracionTotalMin { get; set; }

        public double DiferenciaKm => DistanciaRealKm - DistanciaOptimizadaKm;

        public double EficienciaPorcentaje
        {
            get
            {
                if (DistanciaRealKm <= 0)
                {
                    return 0;
                }

                return (DistanciaOptimizadaKm / DistanciaRealKm) * 100;
            }
        }
    }

    public class EficienciaRutaDetalleDto
    {
        public int RutaId { get; set; }

        public DateOnly Fecha { get; set; }

        public int TecnicoId { get; set; }

        public string Tecnico { get; set; } = string.Empty;

        public int TotalVisitas { get; set; }

        public int VisitasCompletadas { get; set; }

        public double DistanciaOptimizadaKm { get; set; }

        public double DistanciaRealKm { get; set; }

        public int DuracionEstimadaMin { get; set; }

        public double DiferenciaKm => DistanciaRealKm - DistanciaOptimizadaKm;

        public double EficienciaPorcentaje
        {
            get
            {
                if (DistanciaRealKm <= 0)
                {
                    return 0;
                }

                return (DistanciaOptimizadaKm / DistanciaRealKm) * 100;
            }
        }
    }
}