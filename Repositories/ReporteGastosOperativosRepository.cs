using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Repositories
{
    public class ReporteGastosOperativosRepository : IReporteGastosOperativosRepository
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public ReporteGastosOperativosRepository(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<ReporteGastosOperativosDto> ObtenerReporteAsync(
            DateTime? fechaDesde,
            DateTime? fechaHasta,
            int? categoriaId)
        {
            // Query base: solo gastos aprobados
            var query = _context.GastoOperativo
                .Include(g => g.Categoria)
                .Where(g => g.Estado == "Aprobado")
                .AsQueryable();

            // Filtros opcionales
            if (fechaDesde.HasValue)
                query = query.Where(g => g.Fecha >= DateOnly.FromDateTime(fechaDesde.Value));

            if (fechaHasta.HasValue)
                query = query.Where(g => g.Fecha <= DateOnly.FromDateTime(fechaHasta.Value));

            if (categoriaId.HasValue)
                query = query.Where(g => g.CategoriaId == categoriaId.Value);

            // Traer a memoria
            var gastos = await query.ToListAsync();

            // Calcular total general
            decimal totalGeneral = gastos.Sum(g => g.Monto);

            // Agrupar por categoría
            var totalesPorCategoria = gastos
                .GroupBy(g => new { g.CategoriaId, g.Categoria.NombreCategoria })
                .Select(grupo => new TotalPorCategoriaDto
                {
                    CategoriaId = grupo.Key.CategoriaId,
                    NombreCategoria = grupo.Key.NombreCategoria,
                    Total = grupo.Sum(g => g.Monto),
                    Cantidad = grupo.Count(),
                    Porcentaje = totalGeneral == 0 ? 0 : Math.Round(grupo.Sum(g => g.Monto) / totalGeneral * 100, 2)
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            // Detalle de gastos ordenado descendente por fecha
            var detalle = gastos
                .OrderByDescending(g => g.Fecha)
                .Select(g => new GastoDetalleDto
                {
                    Id = g.Id,
                    Fecha = g.Fecha,
                    NombreCategoria = g.Categoria.NombreCategoria,
                    Descripcion = g.Descripcion,
                    Monto = g.Monto
                })
                .ToList();

            return new ReporteGastosOperativosDto
            {
                TotalGeneral = totalGeneral,
                TotalesPorCategoria = totalesPorCategoria,
                Detalle = detalle
            };
        }
    }
}
