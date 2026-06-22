using System;
using System.Collections.Generic;

namespace MultiserviciosPiscinas.Models;

public class PiscinaClienteListViewModel
{
    public int PiscinaId { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public double VolumenM3 { get; set; }

    public string Direccion { get; set; } = string.Empty;

    public string FotoUrl { get; set; } = "/img/piscina-default.jpg";

    public DateTime? ProximaVisitaFechaHora { get; set; }

    public string? ProximaVisitaTipo { get; set; }

    public string? ProximaVisitaEstado { get; set; }
}

public class PiscinaClienteDetalleViewModel
{
    public int PiscinaId { get; set; }

    public string Tipo { get; set; } = string.Empty;

    public string Estado { get; set; } = string.Empty;

    public double VolumenM3 { get; set; }

    public string DireccionCompleta { get; set; } = string.Empty;

    public string FotoUrl { get; set; } = "/img/piscina-default.jpg";

    public DateTime? ProximaVisitaFechaHora { get; set; }

    public string? ProximaVisitaTipo { get; set; }

    public string? ProximaVisitaEstado { get; set; }

    public string? ProximoTecnico { get; set; }

    public List<string> Equipamientos { get; set; } = new();

    public ParametrosPiscinaViewModel? UltimosParametros { get; set; }

    public List<ServicioPiscinaViewModel> UltimosServicios { get; set; } = new();
}

public class ParametrosPiscinaViewModel
{
    public DateOnly FechaInspeccion { get; set; }

    public double? CloroPpm { get; set; }

    public double? Alcalinidad { get; set; }

    public double? Ph { get; set; }

    public double? Calcio { get; set; }

    public double? AcidoCianurico { get; set; }

    public string? Observaciones { get; set; }
}

public class ServicioPiscinaViewModel
{
    public int ServicioId { get; set; }

    public DateTime FechaCita { get; set; }

    public DateOnly FechaApertura { get; set; }

    public DateOnly? FechaCierre { get; set; }

    public string TipoCita { get; set; } = string.Empty;

    public string EstadoServicio { get; set; } = string.Empty;

    public string TrabajoRealizado { get; set; } = string.Empty;

    public string Tecnico { get; set; } = string.Empty;
}