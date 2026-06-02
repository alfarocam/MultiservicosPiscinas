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

        // Inyección a bd
        public PerfilesController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        //  ver lo q es el Perfil con datos reales de la sesión q esta activa
        public async Task<IActionResult> Detalle()
        {
            string? correoLogueado = User.Identity?.Name;

            if (string.IsNullOrEmpty(correoLogueado))
            {
                return Challenge(); // si no hay sesión lo redirecciona al login
            }

            var usuario = await _context.Usuarios
                .Include(u => u.Rol) 
                .FirstOrDefaultAsync(u => u.Correo == correoLogueado);

            if (usuario == null)
            {
                return NotFound("El usuario de la sesión no existe en la base de datos.");
            }

            return View(usuario);
        }

        // Carga formulario de la edición con los datos actuales
        public async Task<IActionResult> Editar()
        {
            string? correoLogueado = User.Identity?.Name;

            if (string.IsNullOrEmpty(correoLogueado))
            {
                return Challenge();
            }

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Correo == correoLogueado);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }

        // Recibo modificaciones y las guardo en el sql
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, string nombre, string correo, string? contrasena)
        {
            var usuarioDb = await _context.Usuarios.FindAsync(id);

            if (usuarioDb == null)
            {
                return NotFound();
            }

            //actualización de campos en clasee usuario 
            usuarioDb.Nombre = nombre;
            usuarioDb.Correo = correo;

            // por si cambia de contra , se le da
            if (!string.IsNullOrEmpty(contrasena))
            {
                usuarioDb.Contrasena = contrasena;
            }

            try
            {
                _context.Update(usuarioDb);
                await _context.SaveChangesAsync(); // Guarda los cambios en sql

                TempData["MensajeExito"] = "¡Perfil actualizado correctamente en la base de datos!";
                return RedirectToAction(nameof(Detalle));
            }
            catch (DbUpdateConcurrencyException)
            {
                ModelState.AddModelError("", "Error de concurrencia al guardar los datos. Inténtalo de nuevo.");
                return View(usuarioDb);
            }
        }
    }
}