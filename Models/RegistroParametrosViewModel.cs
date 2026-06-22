using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models;

public class RegistroParametrosListaViewModel
{
    public List<RegistroParametroItemViewModel> Visitas { get; set; } = new();
}

public class RegistroParametroItemViewModel
{
    public int CitaId { get; set; }

    public int PiscinaId { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Piscina { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public DateTime FechaHora { get; set; }

    public string TipoCita { get; set; } = string.Empty;

    public string EstadoCita { get; set; } = string.Empty;

    public int CantidadMediciones { get; set; }

    public DateOnly? UltimaMedicion { get; set; }
}

public class RegistrarParametroAguaViewModel
{
    public int CitaId { get; set; }

    public int PiscinaId { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Piscina { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public DateTime FechaHoraCita { get; set; }

    public string TipoCita { get; set; } = string.Empty;

    public string EstadoCita { get; set; } = string.Empty;

    public int MedicionesRegistradas { get; set; }

    [Required(ErrorMessage = "La fecha de inspección es obligatoria.")]
    public DateOnly FechaInspeccion { get; set; } = DateOnly.FromDateTime(DateTime.Now);

    [Required(ErrorMessage = "El pH es obligatorio.")]
    [Range(0, 14, ErrorMessage = "El pH debe estar entre 0 y 14.")]
    public double? Ph { get; set; }

    [Required(ErrorMessage = "El cloro es obligatorio.")]
    [Range(0, 10, ErrorMessage = "El cloro debe estar entre 0 y 10 ppm.")]
    public double? CloroPpm { get; set; }

    [Required(ErrorMessage = "La alcalinidad es obligatoria.")]
    [Range(0, 300, ErrorMessage = "La alcalinidad debe estar entre 0 y 300 ppm.")]
    public double? Alcalinidad { get; set; }

    [Required(ErrorMessage = "La temperatura es obligatoria.")]
    [Range(0, 50, ErrorMessage = "La temperatura debe estar entre 0 y 50 °C.")]
    public double? TemperaturaC { get; set; }

    [Range(0, 1000, ErrorMessage = "El calcio debe estar entre 0 y 1000 ppm.")]
    public double? Calcio { get; set; }

    [Range(0, 200, ErrorMessage = "El ácido cianúrico debe estar entre 0 y 200 ppm.")]
    public double? AcidoCianurico { get; set; }

    [StringLength(1000, ErrorMessage = "Las observaciones no pueden superar los 1000 caracteres.")]
    public string? Observaciones { get; set; }
}