namespace MultiserviciosPiscinas.DTOs.Cotizacion;

public class ProductoBusquedaDto
{
    public int Id { get; set; }

    public string Nombre { get; set; } = null!;

    public string? Descripcion { get; set; }

    public decimal Precio { get; set; }

    public int Stock { get; set; }

    public string NombreCategoria { get; set; } = null!;
}
