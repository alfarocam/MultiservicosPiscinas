using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1,2")]
    public class RutaOptimizadaController(
        PiscinasYMultiserviciosContext context,
        IConfiguration configuration) : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context = context;
        private readonly IConfiguration _configuration = configuration;

        private const int ROL_TECNICO = 2;

        [Authorize(Roles = "1")]
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

            model.Paradas = citas.Select((c, index) => new ParadaRutaViewModel
            {
                CitaId = c.Id,
                OrdenVisita = index + 1,
                ClienteNombre = $"{c.Piscina.Cliente.Usuario.Nombre} {c.Piscina.Cliente.Usuario.ApellidoPaterno}",
                Direccion = c.Piscina.Direccion.Detalles,
                Latitud = c.Piscina.Direccion.Latitud,
                Longitud = c.Piscina.Direccion.Longitud,
                HoraCita = c.FechaHora,
                TipoServicio = c.Tipo,
                EstadoCita = c.Estado
            }).ToList();

            return View(model);
        }

        [Authorize(Roles = "2")]
        [HttpGet]
        public async Task<IActionResult> MiRutaDelDia(DateOnly? fecha)
        {
            var usuarioId = await ObtenerUsuarioIdAsync();

            if (usuarioId == null)
            {
                return RedirectToAction("InicioSesion", "Auth");
            }

            var fechaSeleccionada = fecha ?? DateOnly.FromDateTime(DateTime.Today);

            var ruta = await _context.RutaOptimizada
                .Include(r => r.Tecnico)
                .Include(r => r.VisitaRuta)
                    .ThenInclude(v => v.Cita)
                        .ThenInclude(c => c.Piscina)
                            .ThenInclude(p => p.Cliente)
                                .ThenInclude(cl => cl.Usuario)
                .Include(r => r.VisitaRuta)
                    .ThenInclude(v => v.Cita)
                        .ThenInclude(c => c.Piscina)
                            .ThenInclude(p => p.Direccion)
                                .ThenInclude(d => d.Distrito)
                                    .ThenInclude(d => d.Canton)
                                        .ThenInclude(c => c.Provincia)
                .Where(r => r.TecnicoId == usuarioId.Value
                         && r.Fecha == fechaSeleccionada)
                .OrderByDescending(r => r.GeneradaEn)
                .FirstOrDefaultAsync();

            var modelo = new RutaOptimizadaViewModel
            {
                TecnicoId = usuarioId.Value,
                Fecha = fechaSeleccionada,
                GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"] ?? string.Empty
            };

            if (ruta == null)
            {
                var tecnico = await _context.Usuario.FindAsync(usuarioId.Value);

                modelo.TecnicoNombre = tecnico == null
                    ? string.Empty
                    : $"{tecnico.Nombre} {tecnico.ApellidoPaterno}";

                return View(modelo);
            }

            modelo.RutaId = ruta.Id;
            modelo.TecnicoNombre = $"{ruta.Tecnico.Nombre} {ruta.Tecnico.ApellidoPaterno}";
            modelo.DistanciaTotalKm = ruta.DistanciaTotalKm;
            modelo.DuracionTotalMin = ruta.DuracionTotalMin;
            modelo.EnlaceGoogleMaps = ruta.EnlaceGoogleMaps;

            modelo.Paradas = ruta.VisitaRuta
                .OrderBy(v => v.OrdenVisita)
                .Select(v => new ParadaRutaViewModel
                {
                    VisitaRutaId = v.Id,
                    CitaId = v.CitaId,
                    OrdenVisita = v.OrdenVisita,
                    ClienteNombre = $"{v.Cita.Piscina.Cliente.Usuario.Nombre} {v.Cita.Piscina.Cliente.Usuario.ApellidoPaterno}",
                    Direccion = CrearDireccionCompleta(v.Cita.Piscina.Direccion),
                    Latitud = v.Cita.Piscina.Direccion.Latitud,
                    Longitud = v.Cita.Piscina.Direccion.Longitud,
                    HoraCita = v.Cita.FechaHora,
                    TipoServicio = v.Cita.Tipo,
                    EstadoCita = v.Cita.Estado,
                    DistanciaTramoKm = v.DistanciaTramoKm,
                    DuracionTramoMin = v.DuracionTramoMin
                })
                .ToList();

            return View(modelo);
        }

        [Authorize(Roles = "2")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarcarCompletada(int visitaRutaId)
        {
            var usuarioId = await ObtenerUsuarioIdAsync();

            if (usuarioId == null)
            {
                return RedirectToAction("InicioSesion", "Auth");
            }

            var visita = await _context.VisitaRuta
                .Include(v => v.Ruta)
                .Include(v => v.Cita)
                .FirstOrDefaultAsync(v => v.Id == visitaRutaId);

            if (visita == null)
            {
                return NotFound();
            }

            if (visita.Ruta.TecnicoId != usuarioId.Value)
            {
                return Forbid();
            }

            if (visita.Cita.Estado == "Cancelada")
            {
                TempData["MensajeError"] = "No se puede completar una cita cancelada.";
                return RedirectToAction(nameof(MiRutaDelDia), new { fecha = visita.Ruta.Fecha.ToString("yyyy-MM-dd") });
            }

            if (visita.Cita.Estado == "Completada")
            {
                TempData["MensajeError"] = "Esta visita ya se encuentra completada.";
                return RedirectToAction(nameof(MiRutaDelDia), new { fecha = visita.Ruta.Fecha.ToString("yyyy-MM-dd") });
            }

            var estadoAnterior = visita.Cita.Estado;

            visita.Cita.Estado = "Completada";

            _context.BitacoraAuditoria.Add(new BitacoraAuditoria
            {
                UsuarioId = usuarioId.Value,
                Accion = "UPDATE",
                TablaAfectada = "ops.CITA",
                RegistroId = visita.Cita.Id,
                ValorAnterior = $"Estado: {estadoAnterior}",
                ValorNuevo = $"Estado: Completada",
                FechaHora = DateTime.Now
            });

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "La visita fue marcada como completada correctamente.";

            return RedirectToAction(nameof(MiRutaDelDia), new { fecha = visita.Ruta.Fecha.ToString("yyyy-MM-dd") });
        }

        private async Task<int?> ObtenerUsuarioIdAsync()
        {
            var correo = User.FindFirst(ClaimTypes.Email)?.Value
                      ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (string.IsNullOrWhiteSpace(correo))
            {
                return null;
            }

            var usuario = await _context.Usuario
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Correo == correo);

            return usuario?.Id;
        }

        private static string CrearDireccionCompleta(DireccionCliente direccion)
        {
            var partes = new List<string>();

            if (!string.IsNullOrWhiteSpace(direccion.Detalles))
            {
                partes.Add(direccion.Detalles);
            }

            if (direccion.Distrito != null)
            {
                partes.Add(direccion.Distrito.Nombre);

                if (direccion.Distrito.Canton != null)
                {
                    partes.Add(direccion.Distrito.Canton.Nombre);

                    if (direccion.Distrito.Canton.Provincia != null)
                    {
                        partes.Add(direccion.Distrito.Canton.Provincia.Nombre);
                    }
                }
            }

            return string.Join(", ", partes);
        }
    }
}