using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    public class ClientesController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public ClientesController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Cliente
                .Include(c => c.Usuario)
                .Include(c => c.TelefonosCliente)
                .Include(c => c.DireccionCliente)
                .ToListAsync();

            return View(clientes);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var cliente = await _context.Cliente
                .Include(c => c.Usuario)
                .Include(c => c.TelefonosCliente)
                .Include(c => c.DireccionCliente)
                .Include(c => c.Piscina)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        private async Task CargarProvinciasAsync()
        {
            ViewBag.Provincias = await _context.Provincia.OrderBy(p => p.Nombre).ToListAsync();
            ViewBag.Cantones = new List<Canton>();
            ViewBag.Distritos = new List<Distrito>();
        }

        public async Task<IActionResult> Crear()
        {
            await CargarProvinciasAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ClienteCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarProvinciasAsync();
                return View(model);
            }

            bool correoExiste = await _context.Usuario
                .AnyAsync(u => u.Correo == model.Correo);

            if (correoExiste)
            {
                ModelState.AddModelError("", "Ya existe un cliente con ese correo.");
                await CargarProvinciasAsync();
                return View(model);
            }

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                ApellidoPaterno = model.ApellidoPaterno,
                ApellidoMaterno = model.ApellidoMaterno,
                Correo = model.Correo,
                Contrasena = "Temporal123",
                RolId = 2,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();

            var cliente = new Cliente
            {
                UsuarioId = usuario.Id,
                Notas = ""
            };

            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            var telefono = new TelefonosCliente
            {
                ClienteId = cliente.Id,
                TipoTelefono = "Principal",
                NumeroTelefono = model.Telefono,
                EsPrincipal = 1
            };

            _context.TelefonosCliente.Add(telefono);

            var direccion = new DireccionCliente
            {
                ClienteId = cliente.Id,
                DistritoId = model.DistritoId,
                TipoDireccion = "Principal",
                Detalles = model.Direccion,
                EsPrincipal = 1
            };

            _context.DireccionCliente.Add(direccion);

            await _context.SaveChangesAsync();

            TempData["Exito"] = "Cliente registrado correctamente.";

            return RedirectToAction(nameof(Detalle), new { id = cliente.Id });
        }

        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await _context.Cliente
                .Include(c => c.Usuario)
                .Include(c => c.TelefonosCliente)
                .Include(c => c.DireccionCliente)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var cliente = await _context.Cliente
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            cliente.Usuario.Activo = false;

            await _context.SaveChangesAsync();

            TempData["Exito"] = "Cliente desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}