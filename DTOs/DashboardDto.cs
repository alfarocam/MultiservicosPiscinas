namespace MultiserviciosPiscinas.DTOs
{
    //DTO principal que se pasa a la vista Dashboard/Index
    public class DashboardDto
    {
        //KPIs superiores
        public int ClientesActivos { get; set; }
        public int ServiciosEsteMes { get; set; }
        public int FacturasPendientes { get; set; }
        public int TecnicosDisponibles { get; set; }

        //Indicadores de operación (criterios de aceptación - Escenario 2)
        //Rango: trimestre actual; si no hay datos en el trimestre, se usa el año
        public int ServiciosRealizados { get; set; }
        public int ProyectosActivos { get; set; }
        public int VisitasTecnicas { get; set; }
        public string PeriodoKpi { get; set; } = string.Empty; // Ej: "Q3 2026" o "2026"

        //Datos para gráficos
        public List<ServiciosPorMesDto> ServiciosPorMes { get; set; } = new();
        public List<EstadoProyectoDto> EstadosProyectos { get; set; } = new();
        public List<VisitasPorTecnicoDto> VisitasPorTecnico { get; set; } = new();

        //Actividad reciente: últimas 5 citas/servicios
        public List<ActividadRecienteDto> ActividadReciente { get; set; } = new();
    }

    //Gráfico 1: Servicios completados por mes (últimos 6 meses)
    public class ServiciosPorMesDto
    {
        public string Mes { get; set; } = string.Empty;  //"Feb", "Mar"
        public int Cantidad { get; set; }
    }

    //Gráfico 2: Distribución de proyectos por estado
    public class EstadoProyectoDto
    {
        public string Estado { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    //Gráfico 3: Visitas técnicas por técnico (top 5)
    public class VisitasPorTecnicoDto
    {
        public string NombreTecnico { get; set; } = string.Empty;
        public int Cantidad { get; set; }
    }

    //Tabla de actividad reciente
    public class ActividadRecienteDto
    {
        public string Descripcion { get; set; } = string.Empty;  //Cita de mantenimiento
        public string NombreCliente { get; set; } = string.Empty;
        public string NombreTecnico { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string Tipo { get; set; } = string.Empty; //Tipo de cita
    }
}
