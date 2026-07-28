using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Repositories;

public class CarritoRepository : ICarritoRepository
{
    private readonly PiscinasYMultiserviciosContext _context;

    public CarritoRepository(PiscinasYMultiserviciosContext context)
    {
        _context = context;
    }

    public async Task<List<ItemCarritoDto>> ObtenerItemsAsync(int clienteId, decimal tasaIva)
    {
        var carrito = await _context.Carrito
            .Include(c => c.ItemCarrito)
            .ThenInclude(ic => ic.Producto)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (carrito == null)
            return new List<ItemCarritoDto>();

        return carrito.ItemCarrito
            .Select(ic => new ItemCarritoDto
            {
                ProductoId = ic.ProductoId,
                Nombre = ic.Producto.Nombre,
                Descripcion = ic.Producto.Descripcion,
                PrecioUnitario = ic.Producto.Precio,
                Cantidad = ic.Cantidad,
                LineaImpuesto = (ic.Producto.Precio * ic.Cantidad) * tasaIva
            })
            .ToList();
    }

    public async Task<(bool exito, string mensaje)> AgregarItemAsync(int clienteId, int productoId, int cantidad)
    {
        if (cantidad <= 0)
            return (false, "La cantidad debe ser mayor a 0.");

        var producto = await _context.Producto.Include(p => p.Categoria).FirstOrDefaultAsync(p => p.Id == productoId);
        if (producto == null)
            return (false, "Producto no encontrado.");

        // Obtener o crear el carrito del cliente
        var carrito = await _context.Carrito
            .Include(c => c.ItemCarrito)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (carrito == null)
        {
            carrito = new Carrito
            {
                ClienteId = clienteId,
                CreadoEn = DateTime.Now
            };
            _context.Carrito.Add(carrito);
            await _context.SaveChangesAsync();
        }

        var itemExistente = carrito.ItemCarrito.FirstOrDefault(ic => ic.ProductoId == productoId);

        bool esServicio = producto.Categoria?.NombreCategoria?.ToLower().Contains("servicio") ?? false;
        
        if (esServicio)
        {
            if (itemExistente != null)
                return (false, "Este servicio ya se encuentra en el carrito.");
            
            cantidad = 1; // Forzar a 1
        }

        int cantidadTotal = (itemExistente?.Cantidad ?? 0) + cantidad;

        // Validar stock
        if (!esServicio && cantidadTotal > producto.Stock)
            return (false, $"No hay suficiente stock disponible. Existencia: {producto.Stock}.");

        if (itemExistente != null)
        {
            itemExistente.Cantidad = cantidadTotal;
        }
        else
        {
            var nuevoItem = new ItemCarrito
            {
                CarritoId = carrito.Id,
                ProductoId = productoId,
                Cantidad = cantidad
            };
            _context.ItemCarrito.Add(nuevoItem);
        }

        await _context.SaveChangesAsync();
        return (true, "Producto agregado al carrito.");
    }

    public async Task EliminarItemAsync(int clienteId, int productoId)
    {
        var item = await _context.ItemCarrito
            .Include(ic => ic.Carrito)
            .FirstOrDefaultAsync(ic => ic.Carrito.ClienteId == clienteId && ic.ProductoId == productoId);

        if (item != null)
        {
            _context.ItemCarrito.Remove(item);
            await _context.SaveChangesAsync();
        }
    }

    public async Task EliminarCarritoAsync(int clienteId)
    {
        var carrito = await _context.Carrito
            .Include(c => c.ItemCarrito)
            .FirstOrDefaultAsync(c => c.ClienteId == clienteId);

        if (carrito != null)
        {
            // El FK de ITEM_CARRITO no tiene cascade delete, hay que borrar los items primero
            _context.ItemCarrito.RemoveRange(carrito.ItemCarrito);
            _context.Carrito.Remove(carrito);
            await _context.SaveChangesAsync();
        }
    }
}
