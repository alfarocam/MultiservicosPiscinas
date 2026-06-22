using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1,2")]
    public class InventarioController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _contexto;

        public InventarioController(PiscinasYMultiserviciosContext contexto)
        {
            _contexto = contexto;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            await AsegurarCategoriasBasicasAsync();

            var productos = await _contexto.Producto
                .Include(p => p.Categoria)
                .OrderBy(p => p.Id)
                .AsNoTracking()
                .Select(p => new InventarioItemViewModel
                {
                    Id = p.Id,
                    Categoria = p.Categoria.NombreCategoria,
                    Nombre = p.Nombre,
                    Descripcion = p.Descripcion,
                    Precio = p.Precio,
                    Stock = p.Stock,
                    Activo = p.Activo
                })
                .ToListAsync();

            var modelo = new InventarioIndexViewModel
            {
                Productos = productos
            };

            return View(modelo);
        }

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var modelo = new InventarioFormularioViewModel();
            await CargarCategoriasAsync(modelo);

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(InventarioFormularioViewModel modelo)
        {
            await CargarCategoriasAsync(modelo);

            if (!ModelState.IsValid)
                return View(modelo);

            var categoriaExiste = await _contexto.CategoriaProducto
                .AnyAsync(c => c.Id == modelo.CategoriaId!.Value);

            if (!categoriaExiste)
            {
                ModelState.AddModelError(nameof(modelo.CategoriaId), "La categoría seleccionada no existe.");
                return View(modelo);
            }

            var nombreNormalizado = modelo.Nombre.Trim();

            var productoDuplicado = await _contexto.Producto
                .AnyAsync(p =>
                    p.Nombre == nombreNormalizado &&
                    p.CategoriaId == modelo.CategoriaId!.Value &&
                    p.Activo);

            if (productoDuplicado)
            {
                ModelState.AddModelError(nameof(modelo.Nombre), "Ya existe un producto activo con ese nombre en la categoría seleccionada.");
                return View(modelo);
            }

            var producto = new Producto
            {
                CategoriaId = modelo.CategoriaId.Value,
                Nombre = nombreNormalizado,
                Precio = modelo.Precio!.Value,
                Stock = modelo.Stock!.Value,
                Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion)
                    ? null
                    : modelo.Descripcion.Trim(),
                Activo = true
            };

            _contexto.Producto.Add(producto);
            await _contexto.SaveChangesAsync();

            TempData["Exito"] = "El producto fue registrado correctamente en el inventario.";

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var producto = await _contexto.Producto
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            if (producto == null)
                return RedirectToAction(nameof(Index));

            var modelo = new InventarioFormularioViewModel
            {
                Id = producto.Id,
                CategoriaId = producto.CategoriaId,
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Stock = producto.Stock,
                Descripcion = producto.Descripcion,
                Activo = producto.Activo
            };

            await CargarCategoriasAsync(modelo);

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(InventarioFormularioViewModel modelo)
        {
            await CargarCategoriasAsync(modelo);

            if (!ModelState.IsValid)
                return View(modelo);

            var producto = await _contexto.Producto
                .FirstOrDefaultAsync(p => p.Id == modelo.Id);

            if (producto == null)
                return RedirectToAction(nameof(Index));

            var categoriaExiste = await _contexto.CategoriaProducto
                .AnyAsync(c => c.Id == modelo.CategoriaId!.Value);

            if (!categoriaExiste)
            {
                ModelState.AddModelError(nameof(modelo.CategoriaId), "La categoría seleccionada no existe.");
                return View(modelo);
            }

            var nombreNormalizado = modelo.Nombre.Trim();

            var productoDuplicado = await _contexto.Producto
                .AnyAsync(p =>
                    p.Id != modelo.Id &&
                    p.Nombre == nombreNormalizado &&
                    p.CategoriaId == modelo.CategoriaId!.Value &&
                    p.Activo);

            if (productoDuplicado)
            {
                ModelState.AddModelError(nameof(modelo.Nombre), "Ya existe otro producto activo con ese nombre en la categoría seleccionada.");
                return View(modelo);
            }

            producto.CategoriaId = modelo.CategoriaId.Value;
            producto.Nombre = nombreNormalizado;
            producto.Precio = modelo.Precio!.Value;
            producto.Stock = modelo.Stock!.Value;
            producto.Descripcion = string.IsNullOrWhiteSpace(modelo.Descripcion)
                ? null
                : modelo.Descripcion.Trim();
            producto.Activo = modelo.Activo;

            await _contexto.SaveChangesAsync();

            TempData["Exito"] = "El producto fue actualizado correctamente.";

            return RedirectToAction(nameof(Index));
        }

        private async Task CargarCategoriasAsync(InventarioFormularioViewModel modelo)
        {
            await AsegurarCategoriasBasicasAsync();

            modelo.Categorias = await _contexto.CategoriaProducto
                .OrderBy(c => c.NombreCategoria)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.NombreCategoria
                })
                .ToListAsync();
        }

        private async Task AsegurarCategoriasBasicasAsync()
        {
            var existeInsumos = await _contexto.CategoriaProducto
                .AnyAsync(c => c.NombreCategoria == "Insumos");

            var existeHerramientas = await _contexto.CategoriaProducto
                .AnyAsync(c => c.NombreCategoria == "Herramientas");

            if (!existeInsumos)
            {
                _contexto.CategoriaProducto.Add(new CategoriaProducto
                {
                    NombreCategoria = "Insumos"
                });
            }

            if (!existeHerramientas)
            {
                _contexto.CategoriaProducto.Add(new CategoriaProducto
                {
                    NombreCategoria = "Herramientas"
                });
            }

            if (!existeInsumos || !existeHerramientas)
                await _contexto.SaveChangesAsync();
        }
    }
}