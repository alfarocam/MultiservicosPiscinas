using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MultiserviciosPiscinas.Models;
using Microsoft.EntityFrameworkCore;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class ProyectoController(PiscinasYMultiserviciosContext _contexto) : Controller
    {
        private static readonly List<string> EstadosPermitidos =
            ["En Planificación", "En ejecución", "Pausado", "Completado", "Cancelado"];

        #region Index
        [HttpGet]
        public IActionResult Index()
        {
            var piscinas = _contexto.ProyectoConstruccion
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Usuario)
                .OrderBy(p => p.Id)
                .ToList();

            return View(piscinas);
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
            if (model.FechaFinEstimada.HasValue && model.FechaFinEstimada <= model.FechaInicio)
                ModelState.AddModelError(nameof(model.FechaFinEstimada),
                    "La fecha de fin estimada debe ser posterior a la fecha de inicio.");

            if (!ModelState.IsValid)
            {
                await CargarViewBagCrearAsync(model.ClienteId);
                return View(model);
            }

            var proyecto = new ProyectoConstruccion
            {
                ClienteId        = model.ClienteId,
                PiscinaId        = model.PiscinaId,
                Nombre           = model.Nombre,
                Descripcion      = model.Descripcion,
                FechaInicio      = model.FechaInicio,
                FechaFinEstimada = model.FechaFinEstimada,
                Estado           = model.Estado,
                Presupuesto      = model.Presupuesto
            };

            try
            {
                _contexto.ProyectoConstruccion.Add(proyecto);
                await _contexto.SaveChangesAsync();

                TempData["MensajeExito"] = "Proyecto registrado correctamente.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
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
                .Select(p => new { id = p.Id, texto = $"{p.Tipo} — {p.VolumenM3} m³ ({p.Estado})" })
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
                    id     = c.Id,
                    nombre = c.Usuario.Nombre + " " + c.Usuario.ApellidoPaterno + " " + c.Usuario.ApellidoMaterno
                })
                .ToListAsync();

            ViewBag.ClienteSeleccionado = clienteSeleccionado;
            ViewBag.Estados = EstadosPermitidos;

            ViewBag.Piscinas = clienteSeleccionado > 0
                ? await _contexto.Piscina
                    .Where(p => p.ClienteId == clienteSeleccionado)
                    .OrderBy(p => p.Tipo)
                    .Select(p => new { id = p.Id, texto = $"{p.Tipo} — {p.VolumenM3} m³ ({p.Estado})" })
                    .ToListAsync<object>()
                : new List<object>();
        }
        #endregion

        public IActionResult Editar(int id)
        {
            return View();
        }
    }
}
