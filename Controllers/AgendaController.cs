using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;

namespace MultiserviciosPiscinas.Controllers
{
    public class AgendaController(
        PiscinasYMultiserviciosContext context,
        BitacoraService bitacora) : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context = context;
        private readonly BitacoraService _bitacora = bitacora;

        public async Task<IActionResult> Index()
        {
            var citas = await _context.Cita
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Tecnico)
                .Where(c => c.Estado != "Cancelada")
                .OrderByDescending(c => c.FechaHora)
                .ToListAsync();

            return View(citas);
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarViewBagCrearAsync();
            return View();
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CitaCreateViewModel model)
        {
            var piscinaValida = await _context.Piscina
                .AnyAsync(p => p.Id == model.PiscinaId && p.ClienteId == model.ClienteId);

            if (!piscinaValida)
            {
                ModelState.AddModelError(nameof(model.PiscinaId),
                    "La piscina seleccionada no pertenece al cliente indicado.");
            }

            var fechaHora = model.Fecha.ToDateTime(model.Hora);

            var horariosDelDia = await _context.Cita
                .Where(c => c.TecnicoId == model.TecnicoId
                         && c.Estado != "Cancelada"
                         && c.FechaHora.Date == model.Fecha.ToDateTime(TimeOnly.MinValue).Date)
                .Select(c => c.FechaHora)
                .OrderBy(f => f)
                .ToListAsync();

            if (horariosDelDia.Any(h => h == fechaHora))
            {
                var ocupados = string.Join(", ", horariosDelDia.Select(h => h.ToString("HH:mm")));
                ModelState.AddModelError(nameof(model.Hora),
                    $"El técnico ya tiene una cita a esa hora. Horarios ocupados ese día: {ocupados}.");
            }

            if (!ModelState.IsValid)
            {
                await CargarViewBagCrearAsync(model.ClienteId);
                return View(model);
            }

            var cita = new Cita
            {
                PiscinaId = model.PiscinaId,
                TecnicoId = model.TecnicoId,
                FechaHora = fechaHora,
                Tipo = model.Tipo,
                Estado = "Confirmada",
                Notas = string.IsNullOrWhiteSpace(model.Notas) ? null : model.Notas.Trim()
            };

            _context.Cita.Add(cita);
            await _context.SaveChangesAsync();

            _context.Notificacion.Add(new Notificacion
            {
                UsuarioId = model.TecnicoId,
                CitaId = cita.Id,
                Mensaje = $"Se te asignó una nueva cita el {cita.FechaHora:dd/MM/yyyy} a las {cita.FechaHora:HH:mm}.",
                Leida = false,
                FechaCreacion = DateTime.Now
            });
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Cita programada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var cita = await _context.Cita
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Tecnico)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
            {
                return NotFound();
            }

            var model = new CitaEditarViewModel
            {
                Id = cita.Id,
                ClienteNombre = $"{cita.Piscina.Cliente.Usuario.Nombre} {cita.Piscina.Cliente.Usuario.ApellidoPaterno}",
                PiscinaDescripcion = $"{cita.Piscina.Tipo} — {cita.Piscina.VolumenM3} m³",
                TecnicoId = cita.TecnicoId,
                Fecha = DateOnly.FromDateTime(cita.FechaHora),
                Hora = TimeOnly.FromDateTime(cita.FechaHora),
                Estado = cita.Estado,
                Notas = cita.Notas
            };

            await CargarTecnicosAsync();
            await CargarHistorialAsync(id);

            return View(model);
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, CitaEditarViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            var cita = await _context.Cita
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Tecnico)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
            {
                return NotFound();
            }

            var fechaHoraNueva = model.Fecha.ToDateTime(model.Hora);

            var conflicto = await _context.Cita
                .AnyAsync(c => c.Id != id
                             && c.TecnicoId == model.TecnicoId
                             && c.Estado != "Cancelada"
                             && c.FechaHora == fechaHoraNueva);

            if (conflicto)
            {
                ModelState.AddModelError(nameof(model.Hora), "El técnico ya tiene otra cita a esa hora.");
            }

            if (!ModelState.IsValid)
            {
                model.ClienteNombre = $"{cita.Piscina.Cliente.Usuario.Nombre} {cita.Piscina.Cliente.Usuario.ApellidoPaterno}";
                model.PiscinaDescripcion = $"{cita.Piscina.Tipo} — {cita.Piscina.VolumenM3} m³";
                model.Estado = cita.Estado;
                await CargarTecnicosAsync();
                await CargarHistorialAsync(id);
                return View(model);
            }

            var valorAnterior = $"Técnico: {cita.Tecnico.Nombre} {cita.Tecnico.ApellidoPaterno} | Fecha: {cita.FechaHora:dd/MM/yyyy HH:mm}";
            var tecnicoAnteriorId = cita.TecnicoId;

            cita.TecnicoId = model.TecnicoId;
            cita.FechaHora = fechaHoraNueva;
            cita.Notas = string.IsNullOrWhiteSpace(model.Notas) ? null : model.Notas.Trim();

            await _context.SaveChangesAsync();

            var tecnicoNuevo = await _context.Usuario.FindAsync(model.TecnicoId);
            var valorNuevo = $"Técnico: {tecnicoNuevo!.Nombre} {tecnicoNuevo.ApellidoPaterno} | Fecha: {cita.FechaHora:dd/MM/yyyy HH:mm}";

            await _bitacora.RegistrarAsync(
                userClaims: User,
                accion: "UPDATE",
                tablaAfectada: "ops.CITA",
                registroId: cita.Id,
                valorNuevo: valorNuevo,
                valorAnterior: valorAnterior
            );

            _context.Notificacion.Add(new Notificacion
            {
                UsuarioId = model.TecnicoId,
                CitaId = cita.Id,
                Mensaje = $"Se modificó una cita asignada a vos: ahora es el {cita.FechaHora:dd/MM/yyyy} a las {cita.FechaHora:HH:mm}.",
                Leida = false,
                FechaCreacion = DateTime.Now
            });

            if (tecnicoAnteriorId != model.TecnicoId)
            {
                _context.Notificacion.Add(new Notificacion
                {
                    UsuarioId = tecnicoAnteriorId,
                    CitaId = cita.Id,
                    Mensaje = $"Ya no tenés asignada la cita del {cita.FechaHora:dd/MM/yyyy}, fue reasignada a otro técnico.",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }

            _context.Notificacion.Add(new Notificacion
            {
                UsuarioId = cita.Piscina.Cliente.UsuarioId,
                CitaId = cita.Id,
                Mensaje = $"Tu cita fue modificada: ahora es el {cita.FechaHora:dd/MM/yyyy} a las {cita.FechaHora:HH:mm}.",
                Leida = false,
                FechaCreacion = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Cita modificada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id, string motivoCancelacion)
        {
            if (string.IsNullOrWhiteSpace(motivoCancelacion))
            {
                TempData["MensajeError"] = "Debe indicar un motivo de cancelación.";
                return RedirectToAction(nameof(Editar), new { id });
            }

            var cita = await _context.Cita
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cita == null)
            {
                return NotFound();
            }

            var estadoAnterior = cita.Estado;
            cita.Estado = "Cancelada";
            await _context.SaveChangesAsync();

            await _bitacora.RegistrarAsync(
                userClaims: User,
                accion: "UPDATE",
                tablaAfectada: "ops.CITA",
                registroId: cita.Id,
                valorNuevo: $"Estado: Cancelada | Motivo: {motivoCancelacion.Trim()}",
                valorAnterior: $"Estado: {estadoAnterior}"
            );

            _context.Notificacion.Add(new Notificacion
            {
                UsuarioId = cita.TecnicoId,
                CitaId = cita.Id,
                Mensaje = $"Se canceló la cita del {cita.FechaHora:dd/MM/yyyy} a las {cita.FechaHora:HH:mm}. Motivo: {motivoCancelacion.Trim()}",
                Leida = false,
                FechaCreacion = DateTime.Now
            });

            _context.Notificacion.Add(new Notificacion
            {
                UsuarioId = cita.Piscina.Cliente.UsuarioId,
                CitaId = cita.Id,
                Mensaje = $"Tu cita del {cita.FechaHora:dd/MM/yyyy} a las {cita.FechaHora:HH:mm} fue cancelada. Motivo: {motivoCancelacion.Trim()}",
                Leida = false,
                FechaCreacion = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Cita cancelada correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> GetPiscinasPorCliente(int clienteId)
        {
            var piscinas = await _context.Piscina
                .Where(p => p.ClienteId == clienteId)
                .OrderBy(p => p.Tipo)
                .Select(p => new
                {
                    id = p.Id,
                    texto = p.Tipo + " — " + p.VolumenM3 + " m³"
                })
                .ToListAsync();

            return Json(piscinas);
        }

        private async Task CargarViewBagCrearAsync(int clienteSeleccionado = 0)
        {
            ViewBag.Clientes = await _context.Cliente
                .Include(c => c.Usuario)
                .OrderBy(c => c.Usuario.ApellidoPaterno)
                .Select(c => new
                {
                    id = c.Id,
                    nombre = c.Usuario.Nombre + " " + c.Usuario.ApellidoPaterno + " " + c.Usuario.ApellidoMaterno
                })
                .ToListAsync();

            ViewBag.ClienteSeleccionado = clienteSeleccionado;

            ViewBag.Piscinas = clienteSeleccionado > 0
                ? (await _context.Piscina
                    .Where(p => p.ClienteId == clienteSeleccionado)
                    .OrderBy(p => p.Tipo)
                    .Select(p => new
                    {
                        id = p.Id,
                        texto = p.Tipo + " — " + p.VolumenM3 + " m³"
                    })
                    .ToListAsync())
                    .Cast<object>()
                    .ToList()
                : new List<object>();

            await CargarTecnicosAsync();
        }

        private async Task CargarTecnicosAsync()
        {
            ViewBag.Tecnicos = await _context.Usuario
                .Where(u => u.RolId == 2 && u.Activo)
                .OrderBy(u => u.ApellidoPaterno)
                .Select(u => new
                {
                    id = u.Id,
                    nombre = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno
                })
                .ToListAsync();
        }

        private async Task CargarHistorialAsync(int citaId)
        {
            ViewBag.Historial = await _context.BitacoraAuditoria
                .Include(b => b.Usuario)
                .Where(b => b.TablaAfectada == "ops.CITA" && b.RegistroId == citaId)
                .OrderByDescending(b => b.FechaHora)
                .ToListAsync();
        }
    }
}