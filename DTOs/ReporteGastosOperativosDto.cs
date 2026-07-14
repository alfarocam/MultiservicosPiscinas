namespace MultiserviciosPiscinas.DTOs
{
    public class ReporteGastosOperativosDto
    {
        public decimal TotalGeneral { get; set; }
        public List<TotalPorCategoriaDto> TotalesPorCategoria { get; set; } = new();
        public List<GastoDetalleDto> Detalle { get; set; } = new();
    }

    public class TotalPorCategoriaDto
    {
        public int CategoriaId { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public int Cantidad { get; set; }
        public decimal Porcentaje { get; set; }
    }

    public class GastoDetalleDto
    {
        public int Id { get; set; }
        public DateOnly Fecha { get; set; }
        public string NombreCategoria { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal Monto { get; set; }
    }
}
