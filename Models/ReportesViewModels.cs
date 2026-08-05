using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models
{
    public class ReporteServiciosViewModel
    {
        public List<ReporteDetalleServicioDto> Servicios { get; set; } = new List<ReporteDetalleServicioDto>();
    }

    public class ReporteDetalleServicioDto
    {
        public int Id { get; set; }
        public string Cliente { get; set; } = string.Empty;
        public string TipoServicio { get; set; } = string.Empty;
        public string Tecnico { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaHora { get; set; }
    }

    public class ReporteProyectosViewModel
    {
        public List<DetalleProyectoDto> Proyectos { get; set; } = new List<DetalleProyectoDto>();
    }

    public class DetalleProyectoDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Cliente { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateOnly FechaInicio { get; set; }
        public DateOnly? FechaFinEstimada { get; set; }
        public decimal Presupuesto { get; set; }
    }

    public class ReporteRentabilidadViewModel
    {
        public List<DetalleRentabilidadDto> Detalles { get; set; } = new List<DetalleRentabilidadDto>();
        public decimal TotalIngresos { get; set; }
        public decimal TotalGastos { get; set; }
        public decimal RentabilidadTotal => TotalIngresos - TotalGastos;
    }

    public class DetalleRentabilidadDto
    {
        public string Mes { get; set; } = string.Empty;
        public decimal Ingresos { get; set; }
        public decimal Gastos { get; set; }
        public decimal Rentabilidad => Ingresos - Gastos;
    }
}
