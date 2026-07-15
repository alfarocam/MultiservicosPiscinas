using System.ComponentModel.DataAnnotations;

namespace MultiserviciosPiscinas.Models;

public class RegistrarTareasViewModel
{
    public int ServicioId { get; set; }

    public string Cliente { get; set; } = string.Empty;

    public string Piscina { get; set; } = string.Empty;

    public string Direccion { get; set; } = string.Empty;

    public string TipoServicio { get; set; } = string.Empty;

    public string EstadoServicio { get; set; } = string.Empty;

    public DateOnly FechaApertura { get; set; }

    public DateTime FechaCita { get; set; }

    public int CantidadTareas { get; set; }

    public List<TareaServicio> TareasRegistradas { get; set; } = new();

    [Display(Name = "Tarea realizada")]
    [StringLength(500, ErrorMessage = "La tarea no puede superar los 500 caracteres.")]
    public string? NuevaTarea { get; set; }
}