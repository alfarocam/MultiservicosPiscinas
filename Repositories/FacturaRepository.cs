using Microsoft.EntityFrameworkCore;
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
}
