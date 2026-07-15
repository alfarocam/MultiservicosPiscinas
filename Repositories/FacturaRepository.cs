using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.DTOs.Factura;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Repositories;

public class FacturaRepository : IFacturaRepository
{
    private readonly PiscinasYMultiserviciosContext _context;

    public FacturaRepository(PiscinasYMultiserviciosContext context)
    {
        _context = context;
    }

    public async Task<List<CotizacionListadoDto>> ObtenerCotizacionesFacturablesAsync()
    {
        return await _context.Cotizacion
            .Include(c => c.Cliente)
                .ThenInclude(cl => cl.Usuario)
            .Where(c => c.Estado == "Aceptada" && c.Factura == null)
            .OrderByDescending(c => c.FechaEmision)
            .Select(c => new CotizacionListadoDto
            {
                Id = c.Id,
                NumeroCotizacion = $"COT-{c.Id:D5}",
                NombreCliente = c.Cliente.Usuario.Nombre + " " + c.Cliente.Usuario.ApellidoPaterno,
                FechaEmision = c.FechaEmision,
                Total = c.Total,
                Estado = c.Estado
            })
            .ToListAsync();
    }

    public async Task<FacturarCotizacionViewModel?> ObtenerCotizacionParaFacturarAsync(int cotizacionId)
    {
        var cotizacion = await _context.Cotizacion
            .Include(c => c.Cliente)
                .ThenInclude(cl => cl.Usuario)
            .Include(c => c.DetalleCotizacion)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(c => c.Id == cotizacionId && c.Estado == "Aceptada" && c.Factura == null);

        if (cotizacion == null)
            return null;

        return new FacturarCotizacionViewModel
        {
            CotizacionId = cotizacion.Id,
            NumeroCotizacion = $"COT-{cotizacion.Id:D5}",
            NombreCliente = cotizacion.Cliente.Usuario.Nombre + " " + cotizacion.Cliente.Usuario.ApellidoPaterno,
            FechaEmision = cotizacion.FechaEmision,
            Lineas = cotizacion.DetalleCotizacion
                .Select(d => new DetalleFacturarDto
                {
                    ProductoId = d.ProductoId,
                    Nombre = d.Producto.Nombre,
                    Descripcion = d.Producto.Descripcion,
                    PrecioUnitario = d.PrecioUnitario,
                    Cantidad = d.CantidadPropuesta,
                    LineaImpuesto = d.Impuesto
                })
                .ToList(),
            Subtotal = cotizacion.Subtotal,
            ImpuestoTotal = cotizacion.ImpuestoTotal,
            Total = cotizacion.Total
        };
    }

    public async Task<Factura> CrearFacturaAsync(int cotizacionId, string comprobanteRuta, int usuarioId, int diasVencimiento)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var cotizacion = await _context.Cotizacion
                .Include(c => c.DetalleCotizacion)
                .FirstOrDefaultAsync(c => c.Id == cotizacionId)
                ?? throw new InvalidOperationException("Cotización no encontrada.");

            var fechaEmision = DateOnly.FromDateTime(DateTime.Now);

            var factura = new Factura
            {
                ClienteId = cotizacion.ClienteId,
                CotizacionId = cotizacion.Id,
                CreadoPor = usuarioId,
                NumeroConsecutivo = $"TEMP-{Guid.NewGuid():N}",
                FechaEmision = fechaEmision,
                FechaVencimiento = fechaEmision.AddDays(diasVencimiento),
                CondicionPago = "Contado (SINPE)",
                ComprobanteSinpeRuta = comprobanteRuta,
                Subtotal = cotizacion.Subtotal,
                DescuentoTotal = cotizacion.DescuentoTotal,
                ImpuestoTotal = cotizacion.ImpuestoTotal,
                Total = cotizacion.Total,
                Estado = "Pagada"
            };

            _context.Factura.Add(factura);
            await _context.SaveChangesAsync();

            factura.NumeroConsecutivo = $"FAC-{factura.Id:D5}";

            foreach (var detalleCot in cotizacion.DetalleCotizacion)
            {
                _context.DetalleFactura.Add(new DetalleFactura
                {
                    FacturaId = factura.Id,
                    ProductoId = detalleCot.ProductoId,
                    CantidadVendida = detalleCot.CantidadPropuesta,
                    PrecioUnitarioFinal = detalleCot.PrecioUnitario,
                    Descuento = detalleCot.Descuento,
                    Impuesto = detalleCot.Impuesto,
                    LineaSubtotal = detalleCot.LineaSubtotal,
                    LineaTotal = detalleCot.LineaTotal
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return factura;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<Factura> CrearFacturaDesdeCarritoAsync(int clienteId, List<ItemCarritoDto> items, int usuarioId, string comprobanteRuta, int diasVencimiento)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Validar stock de cada producto antes de proceder
            foreach (var item in items)
            {
                var producto = await _context.Producto.FirstOrDefaultAsync(p => p.Id == item.ProductoId)
                    ?? throw new InvalidOperationException($"Producto no encontrado: {item.ProductoId}");

                if (producto.Stock < item.Cantidad)
                    throw new InvalidOperationException($"No hay suficiente stock de '{producto.Nombre}'. Disponible: {producto.Stock}.");

                // Descontar el stock
                producto.Stock -= (int)item.Cantidad;
            }

            var subtotal = items.Sum(i => i.LineaSubtotal);
            var impuestoTotal = items.Sum(i => i.LineaImpuesto);
            var fechaEmision = DateOnly.FromDateTime(DateTime.Now);

            var factura = new Factura
            {
                ClienteId = clienteId,
                CotizacionId = null,
                CreadoPor = usuarioId,
                NumeroConsecutivo = $"TEMP-{Guid.NewGuid():N}",
                FechaEmision = fechaEmision,
                FechaVencimiento = fechaEmision.AddDays(diasVencimiento),
                CondicionPago = "Contado (SINPE)",
                ComprobanteSinpeRuta = comprobanteRuta,
                Subtotal = subtotal,
                DescuentoTotal = 0,
                ImpuestoTotal = impuestoTotal,
                Total = subtotal + impuestoTotal,
                Estado = "Pagada"
            };

            _context.Factura.Add(factura);
            await _context.SaveChangesAsync();

            factura.NumeroConsecutivo = $"FAC-{factura.Id:D5}";

            foreach (var item in items)
            {
                _context.DetalleFactura.Add(new DetalleFactura
                {
                    FacturaId = factura.Id,
                    ProductoId = item.ProductoId,
                    CantidadVendida = item.Cantidad,
                    PrecioUnitarioFinal = item.PrecioUnitario,
                    Descuento = 0,
                    Impuesto = item.LineaImpuesto,
                    LineaSubtotal = item.LineaSubtotal,
                    LineaTotal = item.LineaTotal
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return factura;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<CompraListadoDto>> ObtenerComprasClienteAsync(string correo)
    {
        return await _context.Factura
            .Include(f => f.Cliente)
                .ThenInclude(c => c.Usuario)
            .Where(f => f.Cliente.Usuario.Correo == correo)
            .OrderByDescending(f => f.FechaEmision)
            .Select(f => new CompraListadoDto
            {
                FacturaId = f.Id,
                NumeroFactura = f.NumeroConsecutivo,
                FechaEmision = f.FechaEmision,
                Total = f.Total,
                Estado = f.Estado
            })
            .ToListAsync();
    }

    public async Task<CompraDetalleViewModel?> ObtenerDetalleCompraClienteAsync(int facturaId, string correo)
    {
        var factura = await _context.Factura
            .Include(f => f.Cliente)
                .ThenInclude(c => c.Usuario)
            .Include(f => f.DetalleFactura)
                .ThenInclude(d => d.Producto)
            .FirstOrDefaultAsync(f => f.Id == facturaId && f.Cliente.Usuario.Correo == correo);

        if (factura == null)
            return null;

        return new CompraDetalleViewModel
        {
            Id = factura.Id,
            NumeroFactura = factura.NumeroConsecutivo,
            FechaEmision = factura.FechaEmision,
            FechaVencimiento = factura.FechaVencimiento,
            CondicionPago = factura.CondicionPago,
            ComprobanteSinpeRuta = factura.ComprobanteSinpeRuta,
            Estado = factura.Estado,
            Lineas = factura.DetalleFactura
                .Select(d => new DetalleFacturarDto
                {
                    ProductoId = d.ProductoId,
                    Nombre = d.Producto.Nombre,
                    Descripcion = d.Producto.Descripcion,
                    PrecioUnitario = d.PrecioUnitarioFinal,
                    Cantidad = d.CantidadVendida,
                    LineaImpuesto = d.Impuesto
                })
                .ToList(),
            Subtotal = factura.Subtotal,
            ImpuestoTotal = factura.ImpuestoTotal,
            Total = factura.Total
        };
    }
}
