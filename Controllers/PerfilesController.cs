using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System.Threading.Tasks;

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
                return NotFound("Usuario no encontrado.");
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
                .Include(u => u.Rol)
                .Include(u => u.Cliente)
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
            string? contrasena)
        {
            var usuarioDb = await _context.Usuarios.FindAsync(id);

            if (usuarioDb == null)
            {
                return NotFound();
            }

            usuarioDb.Nombre = nombre;
            usuarioDb.Correo = correo;

            if (!string.IsNullOrWhiteSpace(contrasena))
            {
                usuarioDb.Contrasena = contrasena;
            }

            try
            {
                _context.Update(usuarioDb);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] =
                    "¡Perfil actualizado correctamente!";

                return RedirectToAction(nameof(Detalle));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("",
                    "Error al actualizar el perfil.");

                return View(usuarioDb);
            }
        }
    }
}