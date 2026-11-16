using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MultiserviciosPiscinas.Controllers
{
    // restringe acceso solo el Admin 
    [Authorize(Roles = "1")]
    public class RolesController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;
        private readonly BitacoraService _bitacora;

        public RolesController(PiscinasYMultiserviciosContext context, BitacoraService bitacora)
        {
            _context = context;
            _bitacora = bitacora;
        }

        [HttpGet]
        public async Task<IActionResult> Index(DateTime? fechaModificacion)
        {
            var roles = await _context.Rol.Include(r => r.Usuario).ToListAsync();
            var usuarios = await _context.Usuario
                .Include(u => u.Rol)
                .OrderByDescending(u => u.Id)
                .ToListAsync();

            var ultimasModificaciones = await _context.BitacoraAuditoria
                .Where(b => (b.TablaAfectada == "seg.USUARIO" || b.TablaAfectada == "USUARIO") && b.Accion == "UPDATE")
                .GroupBy(b => b.RegistroId)
                .Select(g => new { UsuarioId = g.Key, UltimaFecha = g.Max(x => x.FechaHora) })
                .ToDictionaryAsync(x => x.UsuarioId, x => (DateTime?)x.UltimaFecha);

            var usuariosVm = usuarios.Select(u => new UsuarioListadoItemViewModel
            {
                Id = u.Id,
                NombreCompleto = $"{u.Nombre} {u.ApellidoPaterno} {u.ApellidoMaterno}".Trim(),
                Correo = u.Correo,
                RolId = u.RolId,
                RolNombre = u.Rol?.Nombre ?? $"Rol {u.RolId}",
                Activo = u.Activo,
                FechaCreacion = u.FechaCreacion,
                FechaModificacion = ultimasModificaciones.GetValueOrDefault(u.Id)
            });

            if (fechaModificacion.HasValue)
            {
                var fechaBuscada = fechaModificacion.Value.Date;
                usuariosVm = usuariosVm.Where(u => u.FechaModificacion.HasValue && u.FechaModificacion.Value.Date == fechaBuscada);
            }

            var viewModel = new RolesIndexViewModel
            {
                Roles = roles,
                Usuarios = usuariosVm.ToList(),
                FechaModificacionFiltro = fechaModificacion
            };

            return View(viewModel);
        }


        private static bool EsCorreoValido(string correo) =>
            Regex.IsMatch(
                correo,
                @"^[a-zA-Z0-9._%+-]+@(gmail\.com|hotmail\.com|yahoo\.com|outlook\.com|icloud\.com|live\.com|piscinas\.com|multiservicios\.com)$",
                RegexOptions.IgnoreCase);

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            var listaRoles = await _context.Rol.ToListAsync();
            ViewBag.Roles = new SelectList(listaRoles, "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario nuevoColaborador)
        {
            nuevoColaborador.Nombre = (nuevoColaborador.Nombre ?? string.Empty).Trim();
            nuevoColaborador.ApellidoPaterno = (nuevoColaborador.ApellidoPaterno ?? string.Empty).Trim();
            nuevoColaborador.ApellidoMaterno = (nuevoColaborador.ApellidoMaterno ?? string.Empty).Trim();
            nuevoColaborador.Correo = (nuevoColaborador.Correo ?? string.Empty).Trim();
            nuevoColaborador.Activo = true;
            nuevoColaborador.FechaCreacion = DateTime.Now;

            if (string.IsNullOrWhiteSpace(nuevoColaborador.Correo) || !EsCorreoValido(nuevoColaborador.Correo))
            {
                ModelState.AddModelError("Correo", "Ingresá un correo válido (@gmail.com, @hotmail.com, @yahoo.com, @outlook.com o institucional).");
                var listaRoles = await _context.Rol.ToListAsync();
                ViewBag.Roles = new SelectList(listaRoles, "Id", "Nombre", nuevoColaborador.RolId);
                return View(nuevoColaborador);
            }

            bool existeCorreo = await _context.Usuario.AnyAsync(u => u.Correo == nuevoColaborador.Correo);
            if (existeCorreo)
            {
                ModelState.AddModelError("Correo", "Este correo electrónico ya se encuentra registrado.");
                var listaRoles = await _context.Rol.ToListAsync();
                ViewBag.Roles = new SelectList(listaRoles, "Id", "Nombre", nuevoColaborador.RolId);
                return View(nuevoColaborador);
            }

            try
            {
                _context.Usuario.Add(nuevoColaborador);
                await _context.SaveChangesAsync();

                await _bitacora.RegistrarAsync(
                    User,
                    "INSERT",
                    "seg.USUARIO",
                    nuevoColaborador.Id,
                    $"Nombre: {nuevoColaborador.Nombre} {nuevoColaborador.ApellidoPaterno} {nuevoColaborador.ApellidoMaterno} | Correo: {nuevoColaborador.Correo} | RolId: {nuevoColaborador.RolId}");

                TempData["MensajeExito"] = $"¡Colaborador '{nuevoColaborador.Nombre}' registrado exitosamente!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error al guardar el registro en  el SQL .");
                var listaRoles = await _context.Rol.ToListAsync();
                ViewBag.Roles = new SelectList(listaRoles, "Id", "Nombre", nuevoColaborador.RolId);
                return View(nuevoColaborador);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var usuario = await _context.Usuario.FindAsync(id);
            if (usuario == null)
            {
                return NotFound();
            }

            await CargarRolesEdicionAsync(usuario.RolId);
            return View(usuario);
        }

        private async Task CargarRolesEdicionAsync(int rolActualId, int? rolSeleccionadoId = null)
        {
            var listaRoles = await _context.Rol.ToListAsync();
            ViewBag.RolActualNombre = listaRoles.FirstOrDefault(r => r.Id == rolActualId)?.Nombre ?? "Sin rol";

            var rolesDisponibles = listaRoles.Where(r => r.Id != rolActualId).ToList();
            ViewBag.Roles = new SelectList(rolesDisponibles, "Id", "Nombre", rolSeleccionadoId);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            int id,
            string nombre,
            string apellidoPaterno,
            string apellidoMaterno,
            string correo,
            int? rolId,
            bool? activo)
        {
            var usuarioDb = await _context.Usuario.FindAsync(id);
            if (usuarioDb == null)
            {
                return NotFound();
            }

            var nombreLimpio = (nombre ?? string.Empty).Trim();
            var apellidoPaternoLimpio = (apellidoPaterno ?? string.Empty).Trim();
            var apellidoMaternoLimpio = (apellidoMaterno ?? string.Empty).Trim();
            var correoLimpio = (correo ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(nombreLimpio))
            {
                ModelState.AddModelError("Nombre", "El nombre es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(apellidoPaternoLimpio))
            {
                ModelState.AddModelError("ApellidoPaterno", "El apellido paterno es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(apellidoMaternoLimpio))
            {
                ModelState.AddModelError("ApellidoMaterno", "El apellido materno es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(correoLimpio))
            {
                ModelState.AddModelError("Correo", "El correo electrónico es obligatorio.");
            }
            else if (!EsCorreoValido(correoLimpio))
            {
                ModelState.AddModelError("Correo", "Ingresá un correo válido (@gmail.com, @hotmail.com, @yahoo.com, @outlook.com o institucional).");
            }
            else if (await _context.Usuario.AnyAsync(u => u.Correo == correoLimpio && u.Id != id))
            {
                ModelState.AddModelError("Correo", "Este correo electrónico ya está registrado por otro usuario.");
            }

            if (rolId.HasValue && (rolId.Value <= 0 || !await _context.Rol.AnyAsync(r => r.Id == rolId.Value)))
            {
                ModelState.AddModelError("RolId", "Debe seleccionar un rol válido.");
            }

            int nuevoRolId = rolId ?? usuarioDb.RolId;
            bool nuevoActivo = activo ?? usuarioDb.Activo;

            if (!ModelState.IsValid)
            {
                await CargarRolesEdicionAsync(usuarioDb.RolId, rolId);
                return View(new Usuario
                {
                    Id = id,
                    Nombre = nombreLimpio,
                    ApellidoPaterno = apellidoPaternoLimpio,
                    ApellidoMaterno = apellidoMaternoLimpio,
                    Correo = correoLimpio,
                    RolId = usuarioDb.RolId,
                    Activo = nuevoActivo
                });
            }

            var valorAnterior = $"Nombre: {usuarioDb.Nombre} {usuarioDb.ApellidoPaterno} {usuarioDb.ApellidoMaterno} | Correo: {usuarioDb.Correo} | RolId: {usuarioDb.RolId} | Activo: {usuarioDb.Activo}";

            usuarioDb.Nombre = nombreLimpio;
            usuarioDb.ApellidoPaterno = apellidoPaternoLimpio;
            usuarioDb.ApellidoMaterno = apellidoMaternoLimpio;
            usuarioDb.Correo = correoLimpio;
            usuarioDb.RolId = nuevoRolId;
            usuarioDb.Activo = nuevoActivo;

            try
            {
                _context.Update(usuarioDb);
                await _context.SaveChangesAsync();

                var valorNuevo = $"Nombre: {usuarioDb.Nombre} {usuarioDb.ApellidoPaterno} {usuarioDb.ApellidoMaterno} | Correo: {usuarioDb.Correo} | RolId: {usuarioDb.RolId} | Activo: {usuarioDb.Activo}";
                await _bitacora.RegistrarAsync(
                    User,
                    "UPDATE",
                    "seg.USUARIO",
                    usuarioDb.Id,
                    valorNuevo,
                    valorAnterior);

                TempData["MensajeExito"] = "Usuario modificado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "Error al guardar los cambios del usuario.");
                await CargarRolesEdicionAsync(usuarioDb.RolId, rolId);
                return View(new Usuario
                {
                    Id = id,
                    Nombre = nombreLimpio,
                    ApellidoPaterno = apellidoPaternoLimpio,
                    ApellidoMaterno = apellidoMaternoLimpio,
                    Correo = correoLimpio,
                    RolId = usuarioDb.RolId,
                    Activo = nuevoActivo
                });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(int id)
        {
            var usuarioDb = await _context.Usuario.FindAsync(id);
            if (usuarioDb == null)
            {
                return NotFound();
            }

            var estadoAnterior = usuarioDb.Activo;
            usuarioDb.Activo = !usuarioDb.Activo;

            try
            {
                _context.Update(usuarioDb);
                await _context.SaveChangesAsync();

                await _bitacora.RegistrarAsync(
                    User,
                    "UPDATE",
                    "seg.USUARIO",
                    usuarioDb.Id,
                    $"Usuario: {usuarioDb.Correo} | Activo: {usuarioDb.Activo}",
                    $"Usuario: {usuarioDb.Correo} | Activo: {estadoAnterior}");

                TempData["MensajeExito"] = usuarioDb.Activo
                    ? $"El usuario '{usuarioDb.Nombre}' ha sido activado exitosamente."
                    : $"El usuario '{usuarioDb.Nombre}' ha sido inactivado exitosamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "Ocurrió un error al intentar cambiar el estado del usuario.";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpGet]
        public async Task<IActionResult> Asignar()
        {
            var usuarios = await _context.Usuario.ToListAsync();
            var roles = await _context.Rol.ToListAsync();

            // Se envían las listas completas (no un SelectList) porque la vista necesita
            // conocer el RolId actual de cada usuario para poder ocultar, en el navegador,
            // el rol que ya tiene asignado dentro del combo de "Nuevo Rol".
            ViewBag.Usuarios = usuarios;
            ViewBag.Roles = roles;

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Asignar(int usuarioId, int rolId)
        {
            //buscar al usuario que se quiere modificar en la BD
            var usuarioDb = await _context.Usuario.FindAsync(usuarioId);

            if (usuarioDb == null)
            {
                return NotFound("El usuario seleccionado no existe.");
            }

            var rolAnterior = await _context.Rol.FindAsync(usuarioDb.RolId);
            var rolNuevo = await _context.Rol.FindAsync(rolId);

            // cambiamos el rol viejo por el nuevo RolId
            usuarioDb.RolId = rolId;

            try
            {
                // guarda sql
                _context.Update(usuarioDb);
                await _context.SaveChangesAsync();

                await _bitacora.RegistrarAsync(
                    userClaims: User,
                    accion: "UPDATE",
                    tablaAfectada: "seg.USUARIO",
                    registroId: usuarioDb.Id,
                    valorNuevo: $"Usuario: {usuarioDb.Correo} | Rol: {rolNuevo?.Nombre ?? rolId.ToString()}",
                    valorAnterior: $"Usuario: {usuarioDb.Correo} | Rol: {rolAnterior?.Nombre ?? "Sin rol"}"
                );

                TempData["MensajeExito"] = "El rol del usuario ha sido modificado con exitoo";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["MensajeError"] = "Ocurrió un error al intentar actualizar el rol.";
                return RedirectToAction(nameof(Index));
            }
        }
    }
}