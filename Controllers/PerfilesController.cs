using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class PerfilesController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public PerfilesController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

 
        public async Task<IActionResult> Detalle()
        {
            string? correoLogueado = User.Identity?.Name;

            if (string.IsNullOrEmpty(correoLogueado))
            {
                return Challenge();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol)
                .Include(u => u.Cliente)
                    .ThenInclude(c => c.TelefonosClientes)
                .FirstOrDefaultAsync(u => u.Correo == correoLogueado);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }


        public async Task<IActionResult> Editar()
        {
            string? correoLogueado = User.Identity?.Name;

            if (string.IsNullOrEmpty(correoLogueado))
            {
                return Challenge();
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Cliente)
                    .ThenInclude(c => c.TelefonosClientes)
                .FirstOrDefaultAsync(u => u.Correo == correoLogueado);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            string nombre,
            string correo,
            string telefono,
            string? contrasena)
        {
            var usuarioDb = await _context.Usuarios
                .Include(u => u.Cliente)
                    .ThenInclude(c => c.TelefonosClientes)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuarioDb == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(nombre))
            {
                ModelState.AddModelError("", "El nombre es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                ModelState.AddModelError("", "El correo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(telefono))
            {
                ModelState.AddModelError("", "El teléfono es obligatorio.");
            }

            if (!ModelState.IsValid)
            {
                return View(usuarioDb);
            }

            usuarioDb.Nombre = nombre;
            usuarioDb.Correo = correo;

            if (!string.IsNullOrWhiteSpace(contrasena))
            {
                usuarioDb.Contrasena = contrasena;
            }

            if (usuarioDb.Cliente != null)
            {
                var telefonoPrincipal = await _context.TelefonosClientes
                    .FirstOrDefaultAsync(t =>
                        t.ClienteId == usuarioDb.Cliente.Id &&
                        t.EsPrincipal == 1);

                if (telefonoPrincipal != null)
                {
                    telefonoPrincipal.NumeroTelefono = telefono;
                    _context.TelefonosClientes.Update(telefonoPrincipal);
                }
                else
                {
                    var nuevoTelefono = new TelefonosCliente
                    {
                        ClienteId = usuarioDb.Cliente.Id,
                        TipoTelefono = "Principal",
                        NumeroTelefono = telefono,
                        EsPrincipal = 1
                    };

                    await _context.TelefonosClientes.AddAsync(nuevoTelefono);
                }
            }

            try
            {
                _context.Update(usuarioDb);
                await _context.SaveChangesAsync(); 

                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "Información actualizada correctamente.";


                return RedirectToAction(nameof(Detalle));
            }
            catch
            {
                ModelState.AddModelError("", "Error de simultaneidad al guardar los datos. Inténtalo de nuevo.");

                ModelState.AddModelError("",
                    "Ocurrió un error al actualizar.");


                return View(usuarioDb);
            }
        }
    }
}