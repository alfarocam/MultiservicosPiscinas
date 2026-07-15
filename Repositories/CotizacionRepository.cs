using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.DTOs.Cotizacion;
using MultiserviciosPiscinas.Interfaces;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Repositories;

public class CotizacionRepository : ICotizacionRepository
{
    private readonly PiscinasYMultiserviciosContext _context;

    public CotizacionRepository(PiscinasYMultiserviciosContext context)
    {
        _context = context;
    }

    public async Task<List<ProductoBusquedaDto>> BuscarProductosAsync(string filtro)
    {
        return await _context.CategoriaProducto
            .OrderBy(c => c.NombreCategoria)
            .Select(c => new CategoriaProductoDto
            {
                Id = c.Id,
                NombreCategoria = c.NombreCategoria
            })
            .ToListAsync();
    }

    public async Task<List<ProductoBusquedaDto>> ObtenerTodosLosProductosAsync()
    {
        return await _context.Producto
            .Include(p => p.Categoria)
            .Where(p => p.Activo)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoBusquedaDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Stock = p.Stock,
                NombreCategoria = p.Categoria.NombreCategoria
            })
            .ToListAsync();
    }

    public async Task<List<ProductoBusquedaDto>> ObtenerProductosPorCategoriaAsync(int categoriaId)
    {
        return await _context.Producto
            .Include(p => p.Categoria)
            .Where(p => p.Activo && p.CategoriaId == categoriaId)
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoBusquedaDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Stock = p.Stock,
                NombreCategoria = p.Categoria.NombreCategoria
            })
            .ToListAsync();
    }

        if (!string.IsNullOrWhiteSpace(filtro))
        {
            query = query.Where(p => p.Nombre.Contains(filtro) ||
                                     (p.Descripcion != null && p.Descripcion.Contains(filtro)) ||
                                     p.Categoria.NombreCategoria.Contains(filtro));
        }

        return await query
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoBusquedaDto
            {
                Id = p.Id,
                Nombre = p.Nombre,
                Descripcion = p.Descripcion,
                Precio = p.Precio,
                Stock = p.Stock,
                NombreCategoria = p.Categoria.NombreCategoria
            })
            .ToListAsync();
    }

    public async Task<ClienteBusquedaDto?> BuscarClientePorCorreoOTelefonoAsync(string valor)
    {
        // Buscar por correo
        var cliente = await _context.Cliente
            .Include(c => c.Usuario)
            .Include(c => c.TelefonosCliente)
            .FirstOrDefaultAsync(c => c.Usuario.Correo == valor);

        if (cliente != null)
        {
            var telefonoPrincipal = cliente.TelefonosCliente
                .FirstOrDefault(t => t.EsPrincipal == 1)?
                .NumeroTelefono ?? cliente.TelefonosCliente.FirstOrDefault()?.NumeroTelefono;

            return new ClienteBusquedaDto
            {
                ClienteId = cliente.Id,
                NombreCompleto = $"{cliente.Usuario.Nombre} {cliente.Usuario.ApellidoPaterno}".Trim(),
                Correo = cliente.Usuario.Correo,
                Telefono = telefonoPrincipal,
                Encontrado = true
            };
        }

        // Buscar por teléfono
        var telefonoCliente = await _context.TelefonosCliente
            .Include(t => t.Cliente)
                .ThenInclude(c => c.Usuario)
            .FirstOrDefaultAsync(t => t.NumeroTelefono == valor);

        if (telefonoCliente?.Cliente != null)
        {
            return new ClienteBusquedaDto
            {
                ClienteId = telefonoCliente.ClienteId,
                NombreCompleto = $"{telefonoCliente.Cliente.Usuario.Nombre} {telefonoCliente.Cliente.Usuario.ApellidoPaterno}".Trim(),
                Correo = telefonoCliente.Cliente.Usuario.Correo,
                Telefono = telefonoCliente.NumeroTelefono,
                Encontrado = true
            };
        }

        return null;
    }

    public async Task<int> RegistrarClienteRapidoAsync(string nombre, string correo, string telefono)
    {
        var contrasenaAleatoria = GenerarContrasenAleatoria();
        var fechaCreacion = DateTime.Now;
        var rolId = 3; // Cliente

        try
        {
            // Ejecutar stored procedure seg.InsertUserAndClient
            var usuarioIdResult = await _context.Database
                .SqlQueryRaw<int>(
                    "EXEC seg.InsertUserAndClient @p0, @p1, @p2, @p3, @p4, @p5, @p6",
                    rolId,
                    nombre,
                    "", // ApellidoPaterno vacío
                    "", // ApellidoMaterno vacío
                    correo,
                    contrasenaAleatoria,
                    fechaCreacion
                )
                .ToListAsync();

            int nuevoUsuarioId = usuarioIdResult.FirstOrDefault();
            if (nuevoUsuarioId <= 0)
                throw new InvalidOperationException("Error al crear usuario.");

            // Insertar registro en cli.CLIENTE
            await _context.Database.ExecuteSqlRawAsync(
                "INSERT INTO cli.CLIENTE (usuario_id, notas) VALUES (@p0, @p1)",
                nuevoUsuarioId,
                "Cliente registrado automáticamente en cotización."
            );

            // Obtener el ClienteId recién creado
            var clienteId = await _context.Cliente
                .Where(c => c.UsuarioId == nuevoUsuarioId)
                .Select(c => c.Id)
                .FirstOrDefaultAsync();

            // Insertar teléfono
            if (!string.IsNullOrWhiteSpace(telefono))
            {
                await _context.Database.ExecuteSqlRawAsync(
                    "INSERT INTO cli.TELEFONOS_CLIENTE (cliente_id, tipo_telefono, numero_telefono, es_principal) VALUES (@p0, @p1, @p2, @p3)",
                    clienteId,
                    "Móvil",
                    telefono,
                    1
                );
            }

            return clienteId;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Error al registrar cliente.", ex);
        }
    }

    public async Task<Cotizacion> CrearCotizacionAsync(int clienteId, List<ItemCarritoDto> items, decimal tasaIva, int diasVigencia)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var fechaEmision = DateOnly.FromDateTime(DateTime.Now);
            var fechaVigencia = fechaEmision.AddDays(diasVigencia);

            decimal subtotal = items.Sum(i => i.LineaSubtotal);
            decimal impuestoTotal = items.Sum(i => i.LineaImpuesto);
            decimal total = subtotal + impuestoTotal;

            var cotizacion = new Cotizacion
            {
                ClienteId = clienteId,
                FechaEmision = fechaEmision,
                FechaVigencia = fechaVigencia,
                Subtotal = subtotal,
                DescuentoTotal = 0,
                ImpuestoTotal = impuestoTotal,
                Total = total,
                Estado = "Pendiente"
            };

            _context.Cotizacion.Add(cotizacion);
            await _context.SaveChangesAsync();

            // Crear detalles de cotización
            foreach (var item in items)
            {
                var detalle = new DetalleCotizacion
                {
                    CotizacionId = cotizacion.Id,
                    ProductoId = item.ProductoId,
                    CantidadPropuesta = item.Cantidad,
                    PrecioUnitario = item.PrecioUnitario,
                    Descuento = 0,
                    Impuesto = item.LineaImpuesto,
                    LineaSubtotal = item.LineaSubtotal,
                    LineaTotal = item.LineaTotal
                };

                _context.DetalleCotizacion.Add(detalle);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return cotizacion;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private string GenerarContrasenAleatoria(int longitud = 12)
    {
        const string caracteres = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789!@#$%";
        var random = new Random();
        return new string(Enumerable.Range(0, longitud)
            .Select(_ => caracteres[random.Next(caracteres.Length)])
            .ToArray());
    }
}
