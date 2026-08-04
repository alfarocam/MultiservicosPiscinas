using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    // Solo el administrador puede optimizar rutas (HU-12.1: "Como usuario administrador...").
    [Authorize(Roles = "1")]
    public class RutaOptimizadaController(
        PiscinasYMultiserviciosContext context,
        IConfiguration configuration) : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        // RolId == 2 identifica a los técnicos en este proyecto (misma convención
        // usada en AgendaController y ServiciosTecnicosController).
        private const int ROL_TECNICO = 2;

        public async Task<IActionResult> Index(int? tecnicoId, DateOnly? fecha)
        {
            var fechaSeleccionada = fecha ?? DateOnly.FromDateTime(DateTime.Today);

            ViewBag.Tecnicos = await _context.Usuario
                .Where(u => u.RolId == ROL_TECNICO && u.Activo)
                .OrderBy(u => u.Nombre)
                .ToListAsync();

            ViewBag.TecnicoId = tecnicoId;
            ViewBag.FechaSeleccionada = fechaSeleccionada;

            var model = new RutaOptimizadaViewModel
            {
                TecnicoId = tecnicoId ?? 0,
                Fecha = fechaSeleccionada,
                GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"] ?? string.Empty
            };

            // Si el administrador todavía no eligió técnico, mostramos la pantalla
            // con el selector, sin paradas.
            if (tecnicoId is null)
            {
                return View(model);
            }

            var inicioDelDia = fechaSeleccionada.ToDateTime(TimeOnly.MinValue);
            var finDelDia = inicioDelDia.AddDays(1);

            var citas = await _context.Cita
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Direccion)
                .Include(c => c.Tecnico)
                .Where(c => c.TecnicoId == tecnicoId
                         && c.Estado != "Cancelada"
                         && c.FechaHora >= inicioDelDia
                         && c.FechaHora < finDelDia)
                .OrderBy(c => c.FechaHora)
                .ToListAsync();

            if (citas.Count > 0)
            {
                model.TecnicoNombre = $"{citas[0].Tecnico.Nombre} {citas[0].Tecnico.ApellidoPaterno}";
            }
            else
            {
                var tecnico = await _context.Usuario.FindAsync(tecnicoId.Value);
                model.TecnicoNombre = tecnico is null
                    ? string.Empty
                    : $"{tecnico.Nombre} {tecnico.ApellidoPaterno}";
            }

            model.Paradas = citas.Select(c => new ParadaRutaViewModel
            {
                CitaId = c.Id,
                ClienteNombre = $"{c.Piscina.Cliente.Usuario.Nombre} {c.Piscina.Cliente.Usuario.ApellidoPaterno}",
                Direccion = c.Piscina.Direccion.Detalles,
                Latitud = c.Piscina.Direccion.Latitud,
                Longitud = c.Piscina.Direccion.Longitud,
                HoraCita = c.FechaHora,
                TipoServicio = c.Tipo
            }).ToList();

            return View(model);
        }
    }
}