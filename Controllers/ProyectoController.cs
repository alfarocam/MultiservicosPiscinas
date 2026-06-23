using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Models;
using Microsoft.EntityFrameworkCore;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class ProyectoController(PiscinasYMultiserviciosContext _contexto) : Controller
    {
        private static readonly List<string> EstadosPermitidos = new()
        {
            "En Planificación",
            "En ejecución",
            "Pausado",
            "Completado",
            "Cancelado"
        };

        private static readonly List<string> EstadosNoEditables = new()
        {
            "Completado",
            "Cancelado"
        };

        #region Index

        [HttpGet]
        public IActionResult Index()
        {
            var proyectos = _contexto.ProyectoConstruccion
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(p => p.Piscina)
                .OrderBy(p => p.Id)
                .ToList();

            return View(proyectos);
        }

        #endregion

        #region Crear Proyecto

        [HttpGet]
        public async Task<IActionResult> Crear()
        {
            await CargarViewBagCrearAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ProyectoCreateViewModel model)
        {
            if (!EstadosPermitidos.Contains(model.Estado))
            {
                ModelState.AddModelError(nameof(model.Estado), "Debe seleccionar un estado válido.");
            }

            if (model.FechaFinEstimada.HasValue && model.FechaFinEstimada <= model.FechaInicio)
            {
                ModelState.AddModelError(nameof(model.FechaFinEstimada),
                    "La fecha de fin estimada debe ser posterior a la fecha de inicio.");
            }

            if (model.PiscinaId.HasValue)
            {
                var piscinaValida = await _contexto.Piscina
                    .AnyAsync(p => p.Id == model.PiscinaId.Value && p.ClienteId == model.ClienteId);

                if (!piscinaValida)
                {
                    ModelState.AddModelError(nameof(model.PiscinaId),
                        "La piscina seleccionada no pertenece al cliente indicado.");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarViewBagCrearAsync(model.ClienteId);
                return View(model);
            }

            var proyecto = new ProyectoConstruccion
            {
                ClienteId = model.ClienteId,
                PiscinaId = model.PiscinaId,
                Nombre = model.Nombre.Trim(),
                Descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim(),
                FechaInicio = model.FechaInicio,
                FechaFinEstimada = model.FechaFinEstimada,
                Estado = model.Estado,
                Presupuesto = model.Presupuesto
            };

            try
            {
                _contexto.ProyectoConstruccion.Add(proyecto);
                await _contexto.SaveChangesAsync();

                TempData["MensajeExito"] = "Proyecto registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                TempData["MensajeError"] = "Ocurrió un error al guardar el proyecto. Por favor, inténtelo de nuevo.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetPiscinasPorCliente(int clienteId)
        {
            var piscinas = await _contexto.Piscina
                .Where(p => p.ClienteId == clienteId)
                .OrderBy(p => p.Tipo)
                .Select(p => new
                {
                    id = p.Id,
                    texto = $"{p.Tipo} — {p.VolumenM3} m³ ({p.Estado})"
                })
                .ToListAsync();

            return Json(piscinas);
        }

        private async Task CargarViewBagCrearAsync(int clienteSeleccionado = 0)
        {
            ViewBag.Clientes = await _contexto.Cliente
                .Include(c => c.Usuario)
                .OrderBy(c => c.Usuario.ApellidoPaterno)
                .Select(c => new
                {
                    id = c.Id,
                    nombre = c.Usuario.Nombre + " " + c.Usuario.ApellidoPaterno + " " + c.Usuario.ApellidoMaterno
                })
                .ToListAsync();

            ViewBag.ClienteSeleccionado = clienteSeleccionado;
            ViewBag.Estados = EstadosPermitidos;

            ViewBag.Piscinas = clienteSeleccionado > 0
                ? await _contexto.Piscina
                    .Where(p => p.ClienteId == clienteSeleccionado)
                    .OrderBy(p => p.Tipo)
                    .Select(p => new
                    {
                        id = p.Id,
                        texto = $"{p.Tipo} — {p.VolumenM3} m³ ({p.Estado})"
                    })
                    .ToListAsync<object>()
                : new List<object>();
        }

        #endregion

        #region Detalle

        [HttpGet]
        public async Task<IActionResult> Detalle(int id)
        {
            var proyecto = await _contexto.ProyectoConstruccion
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.DireccionCliente)
                        .ThenInclude(d => d.Distrito)
                            .ThenInclude(d => d.Canton)
                                .ThenInclude(c => c.Provincia)
                .Include(p => p.Piscina)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proyecto == null)
            {
                TempData["MensajeError"] = "El proyecto solicitado no existe.";
                return RedirectToAction(nameof(Index));
            }

            return View(proyecto);
        }

        #endregion

        #region Editar Proyecto - HU-9.3

        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var proyecto = await _contexto.ProyectoConstruccion
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(p => p.Piscina)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (proyecto == null)
            {
                TempData["MensajeError"] = "El proyecto solicitado no existe.";
                return RedirectToAction(nameof(Index));
            }

            if (!ProyectoEsEditable(proyecto))
            {
                TempData["MensajeError"] = "No se puede editar un proyecto completado o cancelado.";
                return RedirectToAction(nameof(Detalle), new { id = proyecto.Id });
            }

            await CargarViewBagEditarAsync(proyecto);

            var model = new ProyectoEditViewModel
            {
                Id = proyecto.Id,
                ClienteId = proyecto.ClienteId,
                PiscinaId = proyecto.PiscinaId,
                Nombre = proyecto.Nombre ?? string.Empty,
                Descripcion = proyecto.Descripcion,
                FechaInicio = proyecto.FechaInicio,
                FechaFinEstimada = proyecto.FechaFinEstimada,
                Estado = proyecto.Estado ?? string.Empty,
                Presupuesto = proyecto.Presupuesto
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ProyectoEditViewModel model)
        {
            var proyecto = await _contexto.ProyectoConstruccion
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Usuario)
                .FirstOrDefaultAsync(p => p.Id == model.Id);

            if (proyecto == null)
            {
                TempData["MensajeError"] = "El proyecto solicitado no existe.";
                return RedirectToAction(nameof(Index));
            }

            if (!ProyectoEsEditable(proyecto))
            {
                TempData["MensajeError"] = "No se puede editar un proyecto completado o cancelado.";
                return RedirectToAction(nameof(Detalle), new { id = proyecto.Id });
            }

            model.ClienteId = proyecto.ClienteId;

            if (!EstadosPermitidos.Contains(model.Estado))
            {
                ModelState.AddModelError(nameof(model.Estado), "Debe seleccionar un estado válido.");
            }

            if (model.FechaFinEstimada.HasValue && model.FechaFinEstimada <= model.FechaInicio)
            {
                ModelState.AddModelError(nameof(model.FechaFinEstimada),
                    "La fecha de fin estimada debe ser posterior a la fecha de inicio.");
            }

            if (model.PiscinaId.HasValue)
            {
                var piscinaValida = await _contexto.Piscina
                    .AnyAsync(p => p.Id == model.PiscinaId.Value && p.ClienteId == proyecto.ClienteId);

                if (!piscinaValida)
                {
                    ModelState.AddModelError(nameof(model.PiscinaId),
                        "La piscina seleccionada no pertenece al cliente del proyecto.");
                }
            }

            if (!ModelState.IsValid)
            {
                await CargarViewBagEditarAsync(proyecto);
                return View(model);
            }

            try
            {
                proyecto.PiscinaId = model.PiscinaId;
                proyecto.Nombre = model.Nombre.Trim();
                proyecto.Descripcion = string.IsNullOrWhiteSpace(model.Descripcion) ? null : model.Descripcion.Trim();
                proyecto.FechaInicio = model.FechaInicio;
                proyecto.FechaFinEstimada = model.FechaFinEstimada;
                proyecto.Estado = model.Estado;
                proyecto.Presupuesto = model.Presupuesto;

                await _contexto.SaveChangesAsync();

                TempData["MensajeExito"] = "Proyecto actualizado correctamente.";
                return RedirectToAction(nameof(Detalle), new { id = proyecto.Id });
            }
            catch
            {
                await CargarViewBagEditarAsync(proyecto);
                ModelState.AddModelError("", "Ocurrió un error al actualizar el proyecto. Inténtelo nuevamente.");
                return View(model);
            }
        }

        private async Task CargarViewBagEditarAsync(ProyectoConstruccion proyecto)
        {
            ViewBag.NombreCliente =
                $"{proyecto.Cliente.Usuario.Nombre} {proyecto.Cliente.Usuario.ApellidoPaterno} {proyecto.Cliente.Usuario.ApellidoMaterno}".Trim();

            ViewBag.Estados = EstadosPermitidos;

            ViewBag.Piscinas = await _contexto.Piscina
                .Where(p => p.ClienteId == proyecto.ClienteId)
                .OrderBy(p => p.Tipo)
                .Select(p => new
                {
                    id = p.Id,
                    texto = $"{p.Tipo} — {p.VolumenM3} m³ ({p.Estado})"
                })
                .ToListAsync<object>();
        }

        private static bool ProyectoEsEditable(ProyectoConstruccion proyecto)
        {
            return !EstadosNoEditables.Contains(proyecto.Estado ?? string.Empty);
        }

        #endregion
    }
}