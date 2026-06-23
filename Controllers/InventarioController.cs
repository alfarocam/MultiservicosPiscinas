using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1,2")]
    public class InventarioController(
        PiscinasYMultiserviciosContext context,
        BitacoraService bitacora) : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context = context;
        private readonly BitacoraService _bitacora = bitacora;

        // GET: /Inventario
        public async Task<IActionResult> Index(string? busqueda)
        {
            var query = _context.Producto
                .Include(p => p.Categoria)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(p => p.Nombre.Contains(busqueda));

            var productos = await query
                .OrderBy(p => p.Nombre)
                .ToListAsync();

            ViewBag.Busqueda = busqueda;
            return View(productos);
        }

        // GET: /Inventario/Detalle/5
        public async Task<IActionResult> Detalle(int id)
        {
            var producto = await _context.Producto
                .Include(p => p.Categoria)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
                return NotFound();

            return View(producto);
        }

        // GET: /Inventario/Crear
        public async Task<IActionResult> Crear()
        {
            await CargarCategoriasAsync();
            return View();
        }

        // POST: /Inventario/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Producto producto)
        {
            ModelState.Remove(nameof(Producto.Categoria));
            ModelState.Remove(nameof(Producto.DetalleCotizacion));
            ModelState.Remove(nameof(Producto.DetalleFactura));
            ModelState.Remove(nameof(Producto.ItemCarrito));

            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync();
                return View(producto);
            }

            producto.Activo = true;

            _context.Producto.Add(producto);
            await _context.SaveChangesAsync();

            await _bitacora.RegistrarAsync(
                userClaims: User,
                accion: "INSERT",
                tablaAfectada: "inv.PRODUCTO",
                registroId: producto.Id,
                valorNuevo: $"Nombre: {producto.Nombre} | Stock: {producto.Stock} | Precio: {producto.Precio}"
            );

            TempData["MensajeExito"] = "Producto registrado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Inventario/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _context.Producto.FindAsync(id);

            if (producto == null)
                return NotFound();

            await CargarCategoriasAsync(producto.CategoriaId);
            return View(producto);
        }

        // POST: /Inventario/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Producto producto)
        {
            if (id != producto.Id)
                return BadRequest();

            ModelState.Remove(nameof(Producto.Categoria));
            ModelState.Remove(nameof(Producto.DetalleCotizacion));
            ModelState.Remove(nameof(Producto.DetalleFactura));
            ModelState.Remove(nameof(Producto.ItemCarrito));

            if (!ModelState.IsValid)
            {
                await CargarCategoriasAsync(producto.CategoriaId);
                return View(producto);
            }

            var anterior = await _context.Producto.AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            _context.Producto.Update(producto);
            await _context.SaveChangesAsync();

            await _bitacora.RegistrarAsync(
                userClaims: User,
                accion: "UPDATE",
                tablaAfectada: "inv.PRODUCTO",
                registroId: producto.Id,
                valorNuevo: $"Nombre: {producto.Nombre} | Stock: {producto.Stock} | Precio: {producto.Precio}",
                valorAnterior: anterior != null
                    ? $"Nombre: {anterior.Nombre} | Stock: {anterior.Stock} | Precio: {anterior.Precio}"
                    : null
            );

            TempData["MensajeExito"] = "Producto actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCategoriasAsync(int categoriaSeleccionada = 0)
        {
            ViewBag.Categorias = await _context.CategoriaProducto
                .OrderBy(c => c.NombreCategoria)
                .ToListAsync();

            ViewBag.CategoriaSeleccionada = categoriaSeleccionada;
        }
    }
}