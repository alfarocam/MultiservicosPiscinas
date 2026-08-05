using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MultiserviciosPiscinas.Services
{
    public class RecomendacionService : IRecomendacionService
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public RecomendacionService(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<List<ProductoBusquedaDto>> ObtenerRecomendacionesAsync(int? clienteId, int limite = 3)
        {
            if (clienteId.HasValue)
            {
                // Escenario 1 y 3: Obtener categorías de productos comprados previamente por el cliente
                var categoriasCompradas = await _context.DetalleFactura
                    .Where(df => df.Factura.ClienteId == clienteId.Value && df.Factura.Estado != "Anulada")
                    .Select(df => df.Producto.CategoriaId)
                    .Distinct()
                    .ToListAsync();

                if (categoriasCompradas.Any())
                {
                    // Productos que el cliente ya ha comprado
                    var productosCompradosIds = await _context.DetalleFactura
                        .Where(df => df.Factura.ClienteId == clienteId.Value && df.Factura.Estado != "Anulada")
                        .Select(df => df.ProductoId)
                        .Distinct()
                        .ToListAsync();

                    // Buscar productos activos con stock (o que sean servicios) de las categorías compradas que no haya comprado
                    var productosRecomendados = await _context.Producto
                        .Include(p => p.Categoria)
                        .Where(p => p.Activo && (p.Stock > 0 || p.Categoria.NombreCategoria.ToLower().Contains("servicio")) && categoriasCompradas.Contains(p.CategoriaId) && !productosCompradosIds.Contains(p.Id))
                        .OrderBy(p => Guid.NewGuid()) // Aleatorizar un poco la sugerencia
                        .Take(limite)
                        .Select(p => new ProductoBusquedaDto
                        {
                            Id = p.Id,
                            Nombre = p.Nombre,
                            Descripcion = p.Descripcion,
                            Precio = p.Precio,
                            Stock = p.Stock,
                            NombreCategoria = p.Categoria.NombreCategoria,
                            ImagenRuta = p.ImagenRuta
                        })
                        .ToListAsync();
                    
                    // Si encontramos suficientes productos, los retornamos
                    if (productosRecomendados.Count >= limite)
                    {
                        return productosRecomendados;
                    }

                    // Si no hay suficientes, rellenar con otros productos de esas mismas categorías, aunque ya los haya comprado
                    var faltan = limite - productosRecomendados.Count;
                    var productosExtra = await _context.Producto
                        .Include(p => p.Categoria)
                        .Where(p => p.Activo && (p.Stock > 0 || p.Categoria.NombreCategoria.ToLower().Contains("servicio")) && categoriasCompradas.Contains(p.CategoriaId) && productosCompradosIds.Contains(p.Id))
                        .OrderBy(p => Guid.NewGuid())
                        .Take(faltan)
                        .Select(p => new ProductoBusquedaDto
                        {
                            Id = p.Id,
                            Nombre = p.Nombre,
                            Descripcion = p.Descripcion,
                            Precio = p.Precio,
                            Stock = p.Stock,
                            NombreCategoria = p.Categoria.NombreCategoria,
                            ImagenRuta = p.ImagenRuta
                        })
                        .ToListAsync();

                    productosRecomendados.AddRange(productosExtra);

                    if (productosRecomendados.Count > 0)
                    {
                        return productosRecomendados;
                    }
                }
            }

            // Escenario 2: Cliente sin historial de compras
            // Buscar los productos más populares en general (basado en cantidad vendida en todas las facturas válidas)
            var productosMasVendidos = await _context.DetalleFactura
                .Where(df => df.Factura.Estado != "Anulada")
                .GroupBy(df => df.ProductoId)
                .Select(g => new { ProductoId = g.Key, CantidadTotal = g.Sum(x => x.CantidadVendida) })
                .OrderByDescending(x => x.CantidadTotal)
                .Take(limite)
                .Select(x => x.ProductoId)
                .ToListAsync();

            var productosGlobalesRecomendados = new List<ProductoBusquedaDto>();

            if (productosMasVendidos.Any())
            {
                var queryProductos = await _context.Producto
                    .Include(p => p.Categoria)
                    .Where(p => p.Activo && (p.Stock > 0 || p.Categoria.NombreCategoria.ToLower().Contains("servicio")) && productosMasVendidos.Contains(p.Id))
                    .Select(p => new ProductoBusquedaDto
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        Stock = p.Stock,
                        NombreCategoria = p.Categoria.NombreCategoria,
                        ImagenRuta = p.ImagenRuta
                    })
                    .ToListAsync();
                
                // Ordenar según el orden de populares
                productosGlobalesRecomendados = productosMasVendidos
                    .Select(id => queryProductos.FirstOrDefault(p => p.Id == id))
                    .Where(p => p != null)
                    .ToList();
            }

            // Si aún no alcanzamos el límite (por ejemplo, app nueva sin facturas), 
            // llenar con productos aleatorios con stock
            if (productosGlobalesRecomendados.Count < limite)
            {
                var idsActuales = productosGlobalesRecomendados.Select(p => p.Id).ToList();
                var faltan = limite - productosGlobalesRecomendados.Count;

                var randomProds = await _context.Producto
                    .Include(p => p.Categoria)
                    .Where(p => p.Activo && (p.Stock > 0 || p.Categoria.NombreCategoria.ToLower().Contains("servicio")) && !idsActuales.Contains(p.Id))
                    .OrderBy(p => Guid.NewGuid())
                    .Take(faltan)
                    .Select(p => new ProductoBusquedaDto
                    {
                        Id = p.Id,
                        Nombre = p.Nombre,
                        Descripcion = p.Descripcion,
                        Precio = p.Precio,
                        Stock = p.Stock,
                        NombreCategoria = p.Categoria.NombreCategoria,
                        ImagenRuta = p.ImagenRuta
                    })
                    .ToListAsync();

                productosGlobalesRecomendados.AddRange(randomProds);
            }

            return productosGlobalesRecomendados;
        }
    }
}
