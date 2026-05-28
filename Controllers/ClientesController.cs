//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using MultiservicosPiscinas.Data;
//using MultiservicosPiscinas.Models;

//namespace MultiservicosPiscinas.Controllers
//{
    
//    public class ClientesController : Controller
//    {
//        private readonly ApplicationDbContext _context;

//        public ClientesController(ApplicationDbContext context)
//        {
//            _context = context;
//        }

//        // GET: Clientes
//        public async Task<IActionResult> Index()
//        {
//            // Dummy data instead of DB hit
//            var clientes = new List<Cliente>
//            {
//                new Cliente { Id = 1, Nombre = "Juan Perez", Correo = "juan@example.com", Telefono = "555-0001", Direccion = "Calle Falsa 123", Activo = true, FechaRegistro = DateTime.Now.AddDays(-10) },
//                new Cliente { Id = 2, Nombre = "Empresa ABC", Correo = "contacto@empresaabc.com", Telefono = "555-0002", Direccion = "Av. Siempre Viva", Activo = true, FechaRegistro = DateTime.Now.AddDays(-5) }
//            };

//            return View(clientes);
//        }

//        // GET: Clientes/Detalle/5
//        public async Task<IActionResult> Detalle(int id)
//        {
//            var cliente = new Cliente { Id = id, Nombre = "Cliente Simulado", Correo = "simulado@example.com", Telefono = "555-1234", Direccion = "Calle de Prueba", Activo = true, FechaRegistro = DateTime.Now };

//            if (cliente == null) return NotFound();
//            return View(cliente);
//        }

//        // GET: Clientes/Crear
//        public IActionResult Crear()
//        {
//            return View();
//        }

//        // POST: Clientes/Crear
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Crear(Cliente cliente)
//        {
//            if (ModelState.IsValid)
//            {
//                cliente.FechaRegistro = DateTime.Now;
//                _context.Clientes.Add(cliente);
//                await _context.SaveChangesAsync();
//                TempData["Exito"] = "Cliente creado correctamente.";
//                return RedirectToAction(nameof(Index));
//            }
//            return View(cliente);
//        }

//        // GET: Clientes/Editar/5
//        public async Task<IActionResult> Editar(int id)
//        {
//            var cliente = await _context.Clientes.FindAsync(id);
//            if (cliente == null) return NotFound();
//            return View(cliente);
//        }

//        // POST: Clientes/Editar/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Editar(int id, Cliente cliente)
//        {
//            if (id != cliente.Id) return NotFound();

//            if (ModelState.IsValid)
//            {
//                _context.Update(cliente);
//                await _context.SaveChangesAsync();
//                TempData["Exito"] = "Cliente actualizado correctamente.";
//                return RedirectToAction(nameof(Index));
//            }
//            return View(cliente);
//        }

//        // POST: Clientes/Eliminar/5
//        [HttpPost]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> Eliminar(int id)
//        {
//            var cliente = await _context.Clientes.FindAsync(id);
//            if (cliente != null)
//            {
//                cliente.Activo = false; // Baja lógica, no física
//                await _context.SaveChangesAsync();
//                TempData["Exito"] = "Cliente eliminado correctamente.";
//            }
//            return RedirectToAction(nameof(Index));
//        }
//    }
//}
