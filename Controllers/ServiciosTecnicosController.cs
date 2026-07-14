using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1,2")]
    public class ServiciosTecnicosController(PiscinasYMultiserviciosContext context) : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context = context;

        [HttpGet]
        public async Task<IActionResult> Index(string? busqueda)
        {
            var usuarioId = await ObtenerUsuarioIdAsync();

            if (usuarioId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var consulta = _context.Servicio
                .Include(s => s.TareaServicio)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Tecnico)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Cliente)
                            .ThenInclude(cl => cl.Usuario)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Direccion)
                            .ThenInclude(d => d.Distrito)
                .Where(s => s.Estado != "Cancelado")
                .AsQueryable();

            if (User.IsInRole("2"))
            {
                consulta = consulta.Where(s => s.Cita.TecnicoId == usuarioId.Value);
            }

            var servicios = await consulta
                .OrderBy(s => s.Estado == "Cerrado")
                .ThenBy(s => s.FechaApertura)
                .ThenBy(s => s.Cita.FechaHora)
                .AsNoTracking()
                .ToListAsync();

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var texto = busqueda.Trim();

                servicios = servicios.Where(s =>
                    $"{s.Cita.Piscina.Cliente.Usuario.Nombre} {s.Cita.Piscina.Cliente.Usuario.ApellidoPaterno}"
                        .Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || s.Cita.Piscina.Tipo.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || s.Cita.Tipo.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || s.Estado.Contains(texto, StringComparison.OrdinalIgnoreCase)
                    || s.FechaApertura.ToString("dd/MM/yyyy").Contains(texto)
                    || s.Cita.FechaHora.ToString("dd/MM/yyyy").Contains(texto)
                    || (s.FechaCierre.HasValue && s.FechaCierre.Value.ToString("dd/MM/yyyy").Contains(texto))
                ).ToList();
            }

            ViewBag.Busqueda = busqueda;

            return View(servicios);
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var usuarioId = await ObtenerUsuarioIdAsync();

            if (usuarioId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var consulta = _context.Servicio
                .Include(s => s.TareaServicio)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Tecnico)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Cliente)
                            .ThenInclude(cl => cl.Usuario)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Direccion)
                            .ThenInclude(d => d.Distrito)
                                .ThenInclude(d => d.Canton)
                                    .ThenInclude(c => c.Provincia)
                .AsSplitQuery()
                .AsQueryable();

            if (User.IsInRole("2"))
            {
                consulta = consulta.Where(s => s.Cita.TecnicoId == usuarioId.Value);
            }

            var servicio = await consulta
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (servicio == null)
            {
                TempData["MensajeError"] = "No se encontró el servicio asignado o no tiene permiso para consultarlo.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.HistorialServicio = await _context.BitacoraAuditoria
                .Include(b => b.Usuario)
                .Where(b => b.TablaAfectada == "ops.SERVICIO" && b.RegistroId == servicio.Id)
                .OrderByDescending(b => b.FechaHora)
                .AsNoTracking()
                .ToListAsync();

            return View(servicio);
        }

        [HttpGet]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> RegistrarTareas(int id)
        {
            var usuarioId = await ObtenerUsuarioIdAsync();

            if (usuarioId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var servicio = await _context.Servicio
                .Include(s => s.TareaServicio)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Cliente)
                            .ThenInclude(cl => cl.Usuario)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Direccion)
                            .ThenInclude(d => d.Distrito)
                                .ThenInclude(d => d.Canton)
                                    .ThenInclude(c => c.Provincia)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.Cita.TecnicoId == usuarioId.Value);

            if (servicio == null)
            {
                TempData["MensajeError"] = "No se encontró el servicio asignado.";
                return RedirectToAction(nameof(Index));
            }

            if (servicio.Estado == "Cerrado" || servicio.Estado == "Cancelado")
            {
                TempData["MensajeError"] = "No se pueden registrar tareas en un servicio cerrado o cancelado.";
                return RedirectToAction(nameof(Detalle), new { id });
            }

            var modelo = CrearModeloRegistrarTareas(servicio);

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> RegistrarTareas(RegistrarTareasViewModel modelo)
        {
            var usuarioId = await ObtenerUsuarioIdAsync();

            if (usuarioId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var servicio = await _context.Servicio
                .Include(s => s.TareaServicio)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Cliente)
                            .ThenInclude(cl => cl.Usuario)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Direccion)
                            .ThenInclude(d => d.Distrito)
                                .ThenInclude(d => d.Canton)
                                    .ThenInclude(c => c.Provincia)
                .FirstOrDefaultAsync(s =>
                    s.Id == modelo.ServicioId &&
                    s.Cita.TecnicoId == usuarioId.Value);

            if (servicio == null)
            {
                TempData["MensajeError"] = "No se encontró el servicio asignado.";
                return RedirectToAction(nameof(Index));
            }

            CargarModeloRegistrarTareas(modelo, servicio);

            if (servicio.Estado == "Cerrado" || servicio.Estado == "Cancelado")
            {
                ModelState.AddModelError("", "No se pueden registrar tareas en un servicio cerrado o cancelado.");
            }

            if (string.IsNullOrWhiteSpace(modelo.NuevaTarea))
            {
                ModelState.AddModelError(nameof(modelo.NuevaTarea), "Debe describir al menos una tarea.");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var ahora = DateTime.Now;

            var tarea = new TareaServicio
            {
                ServicioId = servicio.Id,
                Descripcion = modelo.NuevaTarea!.Trim(),
                Estado = "Completada",
                FechaAsignacion = DateOnly.FromDateTime(ahora),
                FechaCompletacion = DateOnly.FromDateTime(ahora),
                FechaHoraRegistro = ahora
            };

            _context.TareaServicio.Add(tarea);

            if (servicio.Estado == "Abierto")
            {
                servicio.Estado = "En progreso";
            }

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "La tarea fue registrada correctamente.";
            return RedirectToAction(nameof(RegistrarTareas), new { id = servicio.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Finalizar(int id)
        {
            var usuarioId = await ObtenerUsuarioIdAsync();

            if (usuarioId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var consulta = _context.Servicio
                .Include(s => s.TareaServicio)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Tecnico)
                .Include(s => s.Cita)
                    .ThenInclude(c => c.Piscina)
                        .ThenInclude(p => p.Cliente)
                            .ThenInclude(cl => cl.Usuario)
                .AsQueryable();

            if (User.IsInRole("2"))
            {
                consulta = consulta.Where(s => s.Cita.TecnicoId == usuarioId.Value);
            }

            var servicio = await consulta.FirstOrDefaultAsync(s => s.Id == id);

            if (servicio == null)
            {
                TempData["MensajeError"] = "No se encontró el servicio o no tiene permiso para finalizarlo.";
                return RedirectToAction(nameof(Index));
            }

            if (servicio.Estado == "Cerrado")
            {
                TempData["MensajeError"] = "Este servicio ya está cerrado.";
                return RedirectToAction(nameof(Detalle), new { id = servicio.Id });
            }

            if (servicio.Estado == "Cancelado")
            {
                TempData["MensajeError"] = "No se puede finalizar un servicio cancelado.";
                return RedirectToAction(nameof(Detalle), new { id = servicio.Id });
            }

            var ahora = DateTime.Now;

            var valorAnterior =
                $"ServicioId: {servicio.Id} | Estado servicio: {servicio.Estado} | Fecha cierre: {(servicio.FechaCierre.HasValue ? servicio.FechaCierre.Value.ToString("dd/MM/yyyy") : "Sin fecha")} | Estado cita: {servicio.Cita.Estado}";

            servicio.Estado = "Cerrado";
            servicio.FechaCierre = DateOnly.FromDateTime(ahora);
            servicio.Cita.Estado = "Completada";

            var tareasCompletadas = servicio.TareaServicio
                .Count(t => t.Estado == "Completada");

            var valorNuevo =
                $"ServicioId: {servicio.Id} | Estado servicio: Cerrado | Fecha cierre: {servicio.FechaCierre.Value:dd/MM/yyyy} | Estado cita: Completada | Tareas completadas: {tareasCompletadas} | Cerrado por usuarioId: {usuarioId.Value}";

            _context.BitacoraAuditoria.Add(new BitacoraAuditoria
            {
                UsuarioId = usuarioId.Value,
                Accion = "UPDATE",
                TablaAfectada = "ops.SERVICIO",
                RegistroId = servicio.Id,
                ValorAnterior = valorAnterior,
                ValorNuevo = valorNuevo,
                FechaHora = ahora
            });

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "El servicio fue finalizado correctamente.";
            return RedirectToAction(nameof(Detalle), new { id = servicio.Id });
        }

        private RegistrarTareasViewModel CrearModeloRegistrarTareas(Servicio servicio)
        {
            var modelo = new RegistrarTareasViewModel
            {
                ServicioId = servicio.Id
            };

            CargarModeloRegistrarTareas(modelo, servicio);

            return modelo;
        }

        private static void CargarModeloRegistrarTareas(RegistrarTareasViewModel modelo, Servicio servicio)
        {
            var cliente = servicio.Cita.Piscina.Cliente.Usuario;
            var piscina = servicio.Cita.Piscina;
            var direccion = piscina.Direccion;

            var distrito = direccion?.Distrito?.Nombre ?? "—";
            var canton = direccion?.Distrito?.Canton?.Nombre ?? "—";
            var provincia = direccion?.Distrito?.Canton?.Provincia?.Nombre ?? "—";
            var detalles = direccion?.Detalles ?? "Dirección no registrada";

            modelo.ServicioId = servicio.Id;
            modelo.Cliente = $"{cliente.Nombre} {cliente.ApellidoPaterno} {cliente.ApellidoMaterno}".Trim();
            modelo.Piscina = $"{piscina.Tipo} — {piscina.VolumenM3:0.##} m³";
            modelo.Direccion = $"{detalles}, {distrito}, {canton}, {provincia}";
            modelo.TipoServicio = servicio.Cita.Tipo;
            modelo.EstadoServicio = servicio.Estado;
            modelo.FechaApertura = servicio.FechaApertura;
            modelo.FechaCita = servicio.Cita.FechaHora;
            modelo.CantidadTareas = servicio.TareaServicio.Count;
            modelo.TareasRegistradas = servicio.TareaServicio
                .OrderByDescending(t => t.FechaHoraRegistro ?? t.FechaAsignacion.ToDateTime(TimeOnly.MinValue))
                .ToList();
        }

        private async Task<int?> ObtenerUsuarioIdAsync()
        {
            var correo = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(correo))
                return null;

            return await _context.Usuario
                .Where(u => u.Correo == correo && u.Activo)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();
        }
        [HttpGet]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> MiAgenda(DateOnly? fecha)
        {
            var usuarioId = await ObtenerUsuarioIdAsync();

            if (usuarioId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var fechaConsulta = fecha ?? DateOnly.FromDateTime(DateTime.Today);

            var inicioDia = fechaConsulta.ToDateTime(TimeOnly.MinValue);
            var finDia = inicioDia.AddDays(1);

            var citas = await _context.Cita
                .Include(c => c.Servicio)
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Direccion)
                        .ThenInclude(d => d.Distrito)
                            .ThenInclude(d => d.Canton)
                                .ThenInclude(ca => ca.Provincia)
                .Where(c => c.TecnicoId == usuarioId.Value
                         && c.Estado != "Cancelada"
                         && c.FechaHora >= inicioDia
                         && c.FechaHora < finDia)
                .OrderBy(c => c.FechaHora)
                .AsNoTracking()
                .ToListAsync();

            var modelo = new MiAgendaViewModel
            {
                FechaConsulta = fechaConsulta,
                Citas = citas.Select(c => new MiAgendaCitaViewModel
                {
                    CitaId = c.Id,
                    ServicioId = c.Servicio?.Id,
                    Cliente = $"{c.Piscina.Cliente.Usuario.Nombre} {c.Piscina.Cliente.Usuario.ApellidoPaterno} {c.Piscina.Cliente.Usuario.ApellidoMaterno}".Trim(),
                    Piscina = $"{c.Piscina.Tipo} — {c.Piscina.VolumenM3:0.##} m³",
                    Direccion = CrearDireccionCompleta(c.Piscina.Direccion),
                    TipoServicio = c.Tipo,
                    Estado = c.Estado,
                    FechaHora = c.FechaHora,
                    PuedeReprogramar = c.Estado != "Completada" && c.Estado != "Cancelada"
                }).ToList()
            };

            return View(modelo);
        }

        [HttpGet]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> Reprogramar(int id)
        {
            var usuarioId = await ObtenerUsuarioIdAsync();

            if (usuarioId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var cita = await _context.Cita
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Direccion)
                        .ThenInclude(d => d.Distrito)
                            .ThenInclude(d => d.Canton)
                                .ThenInclude(ca => ca.Provincia)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id && c.TecnicoId == usuarioId.Value);

            if (cita == null)
            {
                TempData["MensajeError"] = "No se encontró la cita asignada.";
                return RedirectToAction(nameof(MiAgenda));
            }

            if (cita.Estado == "Completada" || cita.Estado == "Cancelada")
            {
                TempData["MensajeError"] = "No se puede reprogramar una cita completada o cancelada.";
                return RedirectToAction(nameof(MiAgenda));
            }

            var modelo = CrearModeloReprogramar(cita);

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> Reprogramar(ReprogramarCitaTecnicoViewModel modelo)
        {
            var usuarioId = await ObtenerUsuarioIdAsync();

            if (usuarioId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var cita = await _context.Cita
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Direccion)
                        .ThenInclude(d => d.Distrito)
                            .ThenInclude(d => d.Canton)
                                .ThenInclude(ca => ca.Provincia)
                .Include(c => c.Tecnico)
                .FirstOrDefaultAsync(c => c.Id == modelo.CitaId && c.TecnicoId == usuarioId.Value);

            if (cita == null)
            {
                TempData["MensajeError"] = "No se encontró la cita asignada.";
                return RedirectToAction(nameof(MiAgenda));
            }

            CargarDatosReprogramar(modelo, cita);

            if (cita.Estado == "Completada" || cita.Estado == "Cancelada")
            {
                ModelState.AddModelError("", "No se puede reprogramar una cita completada o cancelada.");
            }

            var nuevaFechaHora = modelo.NuevaFecha.ToDateTime(modelo.NuevaHora);

            if (nuevaFechaHora <= DateTime.Now)
            {
                ModelState.AddModelError(nameof(modelo.NuevaFecha), "La nueva fecha y hora debe ser posterior a la actual.");
            }

            var conflicto = await _context.Cita
                .AnyAsync(c => c.Id != cita.Id
                            && c.TecnicoId == usuarioId.Value
                            && c.Estado != "Cancelada"
                            && c.FechaHora == nuevaFechaHora);

            if (conflicto)
            {
                ModelState.AddModelError(nameof(modelo.NuevaHora), "Ya tienes otra cita programada para esa fecha y hora.");
            }

            if (!ModelState.IsValid)
            {
                return View(modelo);
            }

            var fechaAnterior = cita.FechaHora;
            var estadoAnterior = cita.Estado;

            cita.FechaHora = nuevaFechaHora;
            cita.Estado = "Confirmada";

            if (!string.IsNullOrWhiteSpace(modelo.Motivo))
            {
                var notaReprogramacion = $"Reprogramada por técnico el {DateTime.Now:dd/MM/yyyy HH:mm}. Motivo: {modelo.Motivo.Trim()}";

                cita.Notas = string.IsNullOrWhiteSpace(cita.Notas)
                    ? notaReprogramacion
                    : $"{cita.Notas}\n{notaReprogramacion}";
            }

            _context.BitacoraAuditoria.Add(new BitacoraAuditoria
            {
                UsuarioId = usuarioId.Value,
                Accion = "UPDATE",
                TablaAfectada = "ops.CITA",
                RegistroId = cita.Id,
                ValorAnterior = $"Fecha anterior: {fechaAnterior:dd/MM/yyyy HH:mm} | Estado anterior: {estadoAnterior}",
                ValorNuevo = $"Nueva fecha: {cita.FechaHora:dd/MM/yyyy HH:mm} | Estado: {cita.Estado} | Reprogramada por técnico: {cita.Tecnico.Nombre} {cita.Tecnico.ApellidoPaterno}",
                FechaHora = DateTime.Now
            });

            var administradores = await _context.Usuario
                .Where(u => u.RolId == 1 && u.Activo)
                .ToListAsync();

            foreach (var admin in administradores)
            {
                _context.Notificacion.Add(new Notificacion
                {
                    UsuarioId = admin.Id,
                    CitaId = cita.Id,
                    Mensaje = $"El técnico {cita.Tecnico.Nombre} reprogramó una cita para el {cita.FechaHora:dd/MM/yyyy} a las {cita.FechaHora:HH:mm}.",
                    Leida = false,
                    FechaCreacion = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "La cita fue reprogramada correctamente y se notificó al administrador.";

            return RedirectToAction(nameof(MiAgenda), new
            {
                fecha = DateOnly.FromDateTime(cita.FechaHora).ToString("yyyy-MM-dd")
            });
        }

        private static ReprogramarCitaTecnicoViewModel CrearModeloReprogramar(Cita cita)
        {
            var modelo = new ReprogramarCitaTecnicoViewModel
            {
                CitaId = cita.Id,
                NuevaFecha = DateOnly.FromDateTime(cita.FechaHora),
                NuevaHora = TimeOnly.FromDateTime(cita.FechaHora)
            };

            CargarDatosReprogramar(modelo, cita);

            return modelo;
        }

        private static void CargarDatosReprogramar(ReprogramarCitaTecnicoViewModel modelo, Cita cita)
        {
            modelo.CitaId = cita.Id;
            modelo.Cliente = $"{cita.Piscina.Cliente.Usuario.Nombre} {cita.Piscina.Cliente.Usuario.ApellidoPaterno} {cita.Piscina.Cliente.Usuario.ApellidoMaterno}".Trim();
            modelo.Piscina = $"{cita.Piscina.Tipo} — {cita.Piscina.VolumenM3:0.##} m³";
            modelo.Direccion = CrearDireccionCompleta(cita.Piscina.Direccion);
            modelo.TipoServicio = cita.Tipo;
            modelo.Estado = cita.Estado;
            modelo.FechaHoraActual = cita.FechaHora;
        }

        private static string CrearDireccionCompleta(DireccionCliente? direccion)
        {
            if (direccion == null)
                return "Dirección no registrada";

            var detalles = direccion.Detalles ?? "Sin señas";
            var distrito = direccion.Distrito?.Nombre ?? "—";
            var canton = direccion.Distrito?.Canton?.Nombre ?? "—";
            var provincia = direccion.Distrito?.Canton?.Provincia?.Nombre ?? "—";

            return $"{detalles}, {distrito}, {canton}, {provincia}";
        }
    }

}