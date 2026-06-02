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
            var clientes = await _context.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.TelefonosClientes)
                .Include(c => c.DireccionClientes)
                .ToListAsync();

            return View(clientes);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.TelefonosClientes)
                .Include(c => c.DireccionClientes)
                .Include(c => c.Piscinas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ClienteCreateViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            bool correoExiste = await _context.Usuarios
                .AnyAsync(u => u.Correo == model.Correo);

            if (correoExiste)
            {
                ModelState.AddModelError("", "Ya existe un cliente con ese correo.");
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

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            var cliente = new Cliente
            {
                UsuarioId = usuario.Id,
                Notas = ""
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var telefono = new TelefonosCliente
            {
                ClienteId = cliente.Id,
                TipoTelefono = "Principal",
                NumeroTelefono = model.Telefono,
                EsPrincipal = 1
            };

            _context.TelefonosClientes.Add(telefono);

            var direccion = new DireccionCliente
            {
                ClienteId = cliente.Id,
                DistritoId = 1, // Cambiar por un distrito real
                TipoDireccion = "Principal",
                Detalles = model.Direccion,
                EsPrincipal = 1
            };

            _context.DireccionClientes.Add(direccion);

            await _context.SaveChangesAsync();

            TempData["Exito"] = "Cliente registrado correctamente.";

            return RedirectToAction(nameof(Detalle), new { id = cliente.Id });
        }

        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Usuario)
                .Include(c => c.TelefonosClientes)
                .Include(c => c.DireccionClientes)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var cliente = await _context.Clientes
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