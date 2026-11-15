using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System;
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
        public async Task<IActionResult> Index()
        {
            var roles = await _context.Rol.Include(r => r.Usuario).ToListAsync();
            return View(roles);
        }


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
            nuevoColaborador.Activo = true;
            nuevoColaborador.FechaCreacion = DateTime.Now;

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