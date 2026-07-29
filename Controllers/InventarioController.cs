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
        public async Task<IActionResult> Crear(Producto producto, IFormFile? imagen)
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

            if (imagen != null && imagen.Length > 0)
            {
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(imagen.FileName).ToLower();

                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError("imagen", "Solo se permiten imágenes JPG o PNG.");
                    await CargarCategoriasAsync();
                    return View(producto);
                }

                if (imagen.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("imagen", "El archivo no debe superar 5MB.");
                    await CargarCategoriasAsync();
                    return View(producto);
                }

                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagenes", "productos");
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                producto.ImagenRuta = $"/imagenes/productos/{nombreArchivo}";
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
        public async Task<IActionResult> Editar(int id, Producto producto, IFormFile? imagen)
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

            if (anterior == null)
                return NotFound();

            producto.ImagenRuta = anterior.ImagenRuta; // Mantener imagen anterior por defecto

            if (imagen != null && imagen.Length > 0)
            {
                var extensionesPermitidas = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(imagen.FileName).ToLower();

                if (!extensionesPermitidas.Contains(extension))
                {
                    ModelState.AddModelError("imagen", "Solo se permiten imágenes JPG o PNG.");
                    await CargarCategoriasAsync(producto.CategoriaId);
                    return View(producto);
                }

                if (imagen.Length > 5 * 1024 * 1024)
                {
                    ModelState.AddModelError("imagen", "El archivo no debe superar 5MB.");
                    await CargarCategoriasAsync(producto.CategoriaId);
                    return View(producto);
                }

                var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imagenes", "productos");
                if (!Directory.Exists(carpeta))
                    Directory.CreateDirectory(carpeta);

                var nombreArchivo = $"{Guid.NewGuid()}{extension}";
                var rutaCompleta = Path.Combine(carpeta, nombreArchivo);

                using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                {
                    await imagen.CopyToAsync(stream);
                }

                producto.ImagenRuta = $"/imagenes/productos/{nombreArchivo}";
            }

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var producto = await _context.Producto.FindAsync(id);
            if (producto == null)
                return NotFound();

            producto.Activo = false;
            _context.Producto.Update(producto);
            await _context.SaveChangesAsync();

            await _bitacora.RegistrarAsync(
                userClaims: User,
                accion: "UPDATE",
                tablaAfectada: "inv.PRODUCTO",
                registroId: producto.Id,
                valorNuevo: $"Nombre: {producto.Nombre} | Activo: False",
                valorAnterior: $"Nombre: {producto.Nombre} | Activo: True"
            );

            TempData["MensajeExito"] = "Producto eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activar(int id)
        {
            var producto = await _context.Producto.FindAsync(id);
            if (producto == null)
                return NotFound();

            producto.Activo = true;
            _context.Producto.Update(producto);
            await _context.SaveChangesAsync();

            await _bitacora.RegistrarAsync(
                userClaims: User,
                accion: "UPDATE",
                tablaAfectada: "inv.PRODUCTO",
                registroId: producto.Id,
                valorNuevo: $"Nombre: {producto.Nombre} | Activo: True",
                valorAnterior: $"Nombre: {producto.Nombre} | Activo: False"
            );

            TempData["MensajeExito"] = "Producto activado correctamente.";
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