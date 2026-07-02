namespace MultiserviciosPiscinas.DTOs.Cotizacion;

public class ClienteBusquedaDto
{
    public int ClienteId { get; set; }

    public string NombreCompleto { get; set; } = null!;

    public string Correo { get; set; } = null!;

    public string? Telefono { get; set; }

    public bool Encontrado { get; set; }
}
