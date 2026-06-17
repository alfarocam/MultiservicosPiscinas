using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class PerfilesController(PiscinasYMultiserviciosContext _context) : Controller
    {

        //ver el perfil 
        //get
        //info asociada de roles y clientes
        public async Task<IActionResult> Detalle()
        {
            string? correoLogueado = User.Identity?.Name;

            if (string.IsNullOrEmpty(correoLogueado))
            {
                return Challenge();
            }

            var usuario = await _context.Usuario
                .Include(u => u.Rol)
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.TelefonosCliente)
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.DireccionCliente)
                        .ThenInclude(d => d.Distrito)
                            .ThenInclude(d => d.Canton)
                                .ThenInclude(c => c.Provincia)
                .FirstOrDefaultAsync(u => u.Correo == correoLogueado);

            if (usuario == null)
            {
                return NotFound();
            }

            return View(usuario);
        }
        private async Task CargarListasDireccionAsync(Usuario usuario, int? distritoId = null)
        {
            var provincias = await _context.Provincia.OrderBy(p => p.Nombre).ToListAsync();
            ViewBag.Provincias = provincias;

            ViewBag.SelectedProvinciaId = 0;
            ViewBag.SelectedCantonId = 0;
            ViewBag.SelectedDistritoId = 0;
            ViewBag.DireccionDetalles = "";
            ViewBag.Cantones = new List<Canton>();
            ViewBag.Distritos = new List<Distrito>();

            if (usuario.Cliente != null)
            {
                int? targetDistritoId = distritoId;
                string? detalles = "";

                if (!targetDistritoId.HasValue)
                {
                    var direccionPrincipal = usuario.Cliente.DireccionCliente
                        .FirstOrDefault(d => d.EsPrincipal == 1);

                    if (direccionPrincipal != null)
                    {
                        targetDistritoId = direccionPrincipal.DistritoId;
                        detalles = direccionPrincipal.Detalles;
                    }
                }
                else
                {
                    detalles = ViewBag.DireccionDetalles as string ?? "";
                }

                if (targetDistritoId.HasValue && targetDistritoId.Value > 0)
                {
                    var distritoObj = await _context.Distrito
                        .Include(d => d.Canton)
                        .FirstOrDefaultAsync(d => d.Id == targetDistritoId.Value);

                    if (distritoObj != null)
                    {
                        var cantonObj = distritoObj.Canton;
                        if (cantonObj != null)
                        {
                            ViewBag.SelectedProvinciaId = cantonObj.ProvinciaId;
                            ViewBag.SelectedCantonId = cantonObj.Id;
                            ViewBag.SelectedDistritoId = distritoObj.Id;
                            ViewBag.DireccionDetalles = detalles;

                            ViewBag.Cantones = await _context.Canton
                                .Where(c => c.ProvinciaId == cantonObj.ProvinciaId)
                                .OrderBy(c => c.Nombre)
                                .ToListAsync();

                            ViewBag.Distritos = await _context.Distrito
                                .Where(d => d.CantonId == cantonObj.Id)
                                .OrderBy(d => d.Nombre)
                                .ToListAsync();
                        }
                    }
                }
            }
        }

        //edit
        //
        //cargar el formulario
        public async Task<IActionResult> Editar()
        {
            string? correoLogueado = User.Identity?.Name;

            if (string.IsNullOrEmpty(correoLogueado))
            {
                return Challenge();
            }

            var usuario = await _context.Usuario
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.TelefonosCliente)
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.DireccionCliente)
                .FirstOrDefaultAsync(u => u.Correo == correoLogueado);

            if (usuario == null)
            {
                return NotFound();
            }

            await CargarListasDireccionAsync(usuario);

            return View(usuario);
        }

        //guardar cambios
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            string nombre,
            string correo,
            string telefono,
            int? distritoId,
            string? detalles)
        {
            var usuarioDb = await _context.Usuario
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.TelefonosCliente)
                .Include(u => u.Cliente)
                    .ThenInclude(c => c!.DireccionCliente)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuarioDb == null)
            {
                return NotFound();
            }

            // validaciones
            if (string.IsNullOrWhiteSpace(nombre))
            {
                ModelState.AddModelError("", "El nombre es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(correo))
            {
                ModelState.AddModelError("", "El correo es obligatorio.");
            }

            if (usuarioDb.Cliente != null)
            {
                if (string.IsNullOrWhiteSpace(telefono))
                {
                    ModelState.AddModelError("", "El teléfono es obligatorio.");
                }

                if (!distritoId.HasValue || distritoId.Value <= 0)
                {
                    ModelState.AddModelError("", "Debe seleccionar una provincia, cantón y distrito válidos.");
                }

                if (string.IsNullOrWhiteSpace(detalles))
                {
                    ModelState.AddModelError("", "Los detalles de la dirección son obligatorios.");
                }
            }

            if (!ModelState.IsValid)
            {
                ViewBag.DireccionDetalles = detalles;
                await CargarListasDireccionAsync(usuarioDb, distritoId);
                return View(usuarioDb);
            }

            // act usuario
            usuarioDb.Nombre = nombre;
            usuarioDb.Correo = correo;

            // actualiza tel
            if (usuarioDb.Cliente != null)
            {
                var telefonoPrincipal = usuarioDb.Cliente.TelefonosCliente
                    .FirstOrDefault(t => t.EsPrincipal == 1);

                if (telefonoPrincipal != null)
                {
                    telefonoPrincipal.NumeroTelefono = telefono;
                    _context.TelefonosCliente.Update(telefonoPrincipal);
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
                    await _context.TelefonosCliente.AddAsync(nuevoTelefono);
                }

                // actualiza direccion
                var direccionPrincipal = usuarioDb.Cliente.DireccionCliente
                    .FirstOrDefault(d => d.EsPrincipal == 1);

                if (direccionPrincipal != null)
                {
                    direccionPrincipal.DistritoId = distritoId!.Value;
                    direccionPrincipal.Detalles = detalles!;
                    _context.DireccionCliente.Update(direccionPrincipal);
                }
                else
                {
                    var nuevaDireccion = new DireccionCliente
                    {
                        ClienteId = usuarioDb.Cliente.Id,
                        DistritoId = distritoId!.Value,
                        TipoDireccion = "Principal",
                        Detalles = detalles!,
                        EsPrincipal = 1
                    };
                    await _context.DireccionCliente.AddAsync(nuevaDireccion);
                }
            }

            try
            {
                _context.Update(usuarioDb);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "Información del perfil actualizada correctamente.";
                return RedirectToAction(nameof(Detalle));
            }
            catch
            {
                ModelState.AddModelError("", "Error al guardar los datos. Inténtalo de nuevo.");
                ViewBag.DireccionDetalles = detalles;
                await CargarListasDireccionAsync(usuarioDb, distritoId);
                return View(usuarioDb);
            }
        }

        //cambiar contrasena
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarContrasena(
            int id,
            string contrasena,
            string confirmarContrasena)
        {
            var usuarioDb = await _context.Usuario
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuarioDb == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(contrasena))
            {
                TempData["MensajeErrorContrasena"] = "La nueva contraseña es obligatoria.";
                return RedirectToAction(nameof(Editar));
            }

            if (contrasena != confirmarContrasena)
            {
                TempData["MensajeErrorContrasena"] = "Las contraseñas no coinciden.";
                return RedirectToAction(nameof(Editar));
            }

            usuarioDb.Contrasena = contrasena;

            try
            {
                _context.Update(usuarioDb);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "Contraseña actualizada correctamente.";
                return RedirectToAction(nameof(Detalle));
            }
            catch
            {
                TempData["MensajeErrorContrasena"] = "Ocurrió un error al cambiar la contraseña. Inténtalo de nuevo.";
                return RedirectToAction(nameof(Editar));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetCantones(int provinciaId)
        {
            var cantones = await _context.Canton
                .Where(c => c.ProvinciaId == provinciaId)
                .OrderBy(c => c.Nombre)
                .Select(c => new { id = c.Id, nombre = c.Nombre })
                .ToListAsync();
            return Json(cantones);
        }

        [HttpGet]
        public async Task<IActionResult> GetDistritos(int cantonId)
        {
            var distritos = await _context.Distrito
                .Where(d => d.CantonId == cantonId)
                .OrderBy(d => d.Nombre)
                .Select(d => new { id = d.Id, nombre = d.Nombre })
                .ToListAsync();
            return Json(distritos);
        }
    }
}