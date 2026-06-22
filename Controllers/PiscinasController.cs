using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class PiscinasController(PiscinasYMultiserviciosContext _contexto) : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            if (User.IsInRole("2"))
                return RedirectToAction(nameof(RegistroParametros));

            if (User.IsInRole("3"))
                return RedirectToAction(nameof(MisPiscinas));

            var piscinas = _contexto.Piscina
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(p => p.Direccion)
                    .ThenInclude(d => d.Distrito)
                .OrderBy(p => p.Id)
                .ToList();

            return View(piscinas);
        }

        [HttpGet]
        public IActionResult Crear(int clienteId)
        {
            var cliente = _contexto.Cliente
                .Include(c => c.Usuario)
                .Include(c => c.DireccionCliente)
                    .ThenInclude(d => d.Distrito)
                .FirstOrDefault(c => c.Id == clienteId);

            if (cliente == null)
                return RedirectToAction("Index", "Clientes");

            ViewBag.NombreCliente = $"{cliente.Usuario.Nombre} {cliente.Usuario.ApellidoPaterno}";
            ViewBag.ClienteId = clienteId;
            ViewBag.Direcciones = new SelectList(
                cliente.DireccionCliente.Select(d => new
                {
                    d.Id,
                    Texto = $"{d.TipoDireccion} — {d.Detalles}, {d.Distrito.Nombre}"
                }),
                "Id",
                "Texto"
            );

            return View();
        }

        [HttpPost]
        public IActionResult Crear(Piscina piscina)
        {
            ModelState.Remove("Cliente");
            ModelState.Remove("Direccion");

            if (ModelState.IsValid)
            {
                _contexto.Piscina.Add(piscina);
                _contexto.SaveChanges();
                return RedirectToAction("Index", "Piscinas");
            }

            var clienteConDatos = _contexto.Cliente
                .Include(c => c.Usuario)
                .Include(c => c.DireccionCliente)
                    .ThenInclude(d => d.Distrito)
                .FirstOrDefault(c => c.Id == piscina.ClienteId);

            if (clienteConDatos != null)
            {
                ViewBag.NombreCliente = $"{clienteConDatos.Usuario.Nombre} {clienteConDatos.Usuario.ApellidoPaterno}";
                ViewBag.ClienteId = piscina.ClienteId;
                ViewBag.Direcciones = new SelectList(
                    clienteConDatos.DireccionCliente.Select(d => new
                    {
                        d.Id,
                        Texto = $"{d.TipoDireccion} — {d.Detalles}, {d.Distrito.Nombre}"
                    }),
                    "Id",
                    "Texto"
                );
            }

            return View(piscina);
        }

        [HttpGet]
        public IActionResult Historial(int id)
        {
            var piscina = _contexto.Piscina
                .Include(p => p.Cliente).ThenInclude(c => c.Usuario)
                .Include(p => p.Cita).ThenInclude(c => c.Tecnico)
                .Include(p => p.Cita).ThenInclude(c => c.Servicio).ThenInclude(s => s!.Inspeccion)
                .Include(p => p.Cita).ThenInclude(c => c.Servicio).ThenInclude(s => s!.TareaServicio)
                .FirstOrDefault(p => p.Id == id);

            if (piscina == null)
                return RedirectToAction("Index");

            return View(piscina);
        }

        [HttpGet]
        public IActionResult Detalle(int id)
        {
            var piscina = _contexto.Piscina
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(p => p.Direccion)
                    .ThenInclude(d => d.Distrito)
                        .ThenInclude(d => d.Canton)
                            .ThenInclude(c => c.Provincia)
                .FirstOrDefault(p => p.Id == id);

            if (piscina == null)
                return RedirectToAction("Index");

            return View(piscina);
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            var piscina = _contexto.Piscina
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.DireccionCliente)
                        .ThenInclude(d => d.Distrito)
                .FirstOrDefault(p => p.Id == id);

            if (piscina == null)
                return RedirectToAction("Index");

            CargarViewBagEditar(piscina.Cliente, piscina.DireccionId, piscina.Tipo, piscina.Estado);
            return View(piscina);
        }

        [HttpPost]
        public IActionResult Editar(Piscina piscina)
        {
            ModelState.Remove("Cliente");
            ModelState.Remove("Direccion");

            if (ModelState.IsValid)
            {
                var piscinaExistente = _contexto.Piscina.Find(piscina.Id);

                if (piscinaExistente == null)
                    return RedirectToAction("Index");

                piscinaExistente.DireccionId = piscina.DireccionId;
                piscinaExistente.Tipo = piscina.Tipo;
                piscinaExistente.VolumenM3 = piscina.VolumenM3;
                piscinaExistente.Estado = piscina.Estado;

                _contexto.SaveChanges();
                return RedirectToAction("Index");
            }

            var clienteParaNavegacion = _contexto.Cliente
                .Include(c => c.Usuario)
                .Include(c => c.DireccionCliente)
                    .ThenInclude(d => d.Distrito)
                .FirstOrDefault(c => c.Id == piscina.ClienteId);

            if (clienteParaNavegacion != null)
                CargarViewBagEditar(clienteParaNavegacion, piscina.DireccionId, piscina.Tipo, piscina.Estado);

            return View(piscina);
        }

        // ======================================================
        // HU-4.3 - Registrar parámetros del agua
        // Técnico
        // ======================================================

        [HttpGet]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> RegistroParametros()
        {
            var tecnicoId = await ObtenerUsuarioAutenticadoIdAsync();

            if (tecnicoId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var citas = await _contexto.Cita
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Direccion)
                        .ThenInclude(d => d.Distrito)
                .Include(c => c.Servicio)
                    .ThenInclude(s => s!.Inspeccion)
                .Where(c =>
                    c.TecnicoId == tecnicoId.Value &&
                    c.Estado != "Cancelada" &&
                    c.Estado != "Completada" &&
                    (c.Tipo == "Mantenimiento" || c.Tipo == "Inspección"))
                .OrderBy(c => c.FechaHora)
                .AsNoTracking()
                .ToListAsync();

            var modelo = new RegistroParametrosListaViewModel
            {
                Visitas = citas.Select(c => new RegistroParametroItemViewModel
                {
                    CitaId = c.Id,
                    PiscinaId = c.PiscinaId,
                    Cliente = $"{c.Piscina.Cliente.Usuario.Nombre} {c.Piscina.Cliente.Usuario.ApellidoPaterno}",
                    Piscina = $"Piscina #{c.Piscina.Id} - {c.Piscina.Tipo}",
                    Direccion = ConstruirDireccionCorta(c.Piscina.Direccion),
                    FechaHora = c.FechaHora,
                    TipoCita = c.Tipo,
                    EstadoCita = c.Estado,
                    CantidadMediciones = c.Servicio?.Inspeccion.Count ?? 0,
                    UltimaMedicion = c.Servicio?.Inspeccion
                        .OrderByDescending(i => i.FechaInspeccion)
                        .Select(i => (DateOnly?)i.FechaInspeccion)
                        .FirstOrDefault()
                }).ToList()
            };

            return View(modelo);
        }

        [HttpGet]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> RegistrarParametro(int citaId)
        {
            var tecnicoId = await ObtenerUsuarioAutenticadoIdAsync();

            if (tecnicoId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var cita = await _contexto.Cita
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Direccion)
                        .ThenInclude(d => d.Distrito)
                .Include(c => c.Servicio)
                    .ThenInclude(s => s!.Inspeccion)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == citaId && c.TecnicoId == tecnicoId.Value);

            if (cita == null)
                return RedirectToAction(nameof(RegistroParametros));

            var modelo = new RegistrarParametroAguaViewModel
            {
                CitaId = cita.Id,
                FechaInspeccion = DateOnly.FromDateTime(DateTime.Now)
            };

            CargarDatosFormularioParametro(modelo, cita);

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "2")]
        public async Task<IActionResult> RegistrarParametro(RegistrarParametroAguaViewModel modelo)
        {
            var tecnicoId = await ObtenerUsuarioAutenticadoIdAsync();

            if (tecnicoId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var cita = await _contexto.Cita
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Cliente)
                        .ThenInclude(cl => cl.Usuario)
                .Include(c => c.Piscina)
                    .ThenInclude(p => p.Direccion)
                        .ThenInclude(d => d.Distrito)
                .Include(c => c.Servicio)
                    .ThenInclude(s => s!.Inspeccion)
                .FirstOrDefaultAsync(c => c.Id == modelo.CitaId && c.TecnicoId == tecnicoId.Value);

            if (cita == null)
            {
                TempData["Error"] = "No se encontró la visita asignada.";
                return RedirectToAction(nameof(RegistroParametros));
            }

            CargarDatosFormularioParametro(modelo, cita);

            if (cita.Estado == "Cancelada" || cita.Estado == "Completada")
            {
                ModelState.AddModelError("", "No se pueden registrar parámetros en una cita cancelada o completada.");
            }

            if (cita.Tipo != "Mantenimiento" && cita.Tipo != "Inspección")
            {
                ModelState.AddModelError("", "Solo se pueden registrar parámetros en visitas de mantenimiento o inspección.");
            }

            if (!ModelState.IsValid)
                return View(modelo);

            await using var transaccion = await _contexto.Database.BeginTransactionAsync();

            try
            {
                Servicio servicio;

                if (cita.Servicio == null)
                {
                    servicio = new Servicio
                    {
                        CitaId = cita.Id,
                        FechaApertura = DateOnly.FromDateTime(DateTime.Now),
                        Estado = "En progreso",
                        TrabajoRealizado = "Registro de parámetros del agua."
                    };

                    _contexto.Servicio.Add(servicio);
                    await _contexto.SaveChangesAsync();
                }
                else
                {
                    servicio = cita.Servicio;

                    if (servicio.Estado == "Abierto")
                        servicio.Estado = "En progreso";
                }

                var inspeccion = new Inspeccion
                {
                    ServicioId = servicio.Id,
                    FechaInspeccion = modelo.FechaInspeccion,
                    Ph = modelo.Ph,
                    CloroPpm = modelo.CloroPpm,
                    Alcalinidad = modelo.Alcalinidad,
                    TemperaturaC = modelo.TemperaturaC,
                    Calcio = modelo.Calcio,
                    AcidoCianurico = modelo.AcidoCianurico,
                    Observaciones = modelo.Observaciones
                };

                _contexto.Inspeccion.Add(inspeccion);

                if (cita.Estado == "Pendiente" || cita.Estado == "Confirmada" || cita.Estado == "En camino")
                    cita.Estado = "En progreso";

                await _contexto.SaveChangesAsync();
                await transaccion.CommitAsync();

                TempData["Exito"] = "La medición del agua se registró correctamente y el historial fue actualizado.";
                return RedirectToAction(nameof(RegistroParametros));
            }
            catch
            {
                await transaccion.RollbackAsync();
                ModelState.AddModelError("", "Ocurrió un error al guardar la medición. Intenta nuevamente.");
                return View(modelo);
            }
        }

        // ======================================================
        // HU-4.2 - Consultar mis piscinas como cliente
        // ======================================================

        [HttpGet]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> MisPiscinas()
        {
            var clienteId = await ObtenerClienteIdAutenticadoAsync();

            if (clienteId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var ahora = DateTime.Now;

            var piscinas = await _contexto.Piscina
                .Include(p => p.Direccion)
                    .ThenInclude(d => d.Distrito)
                .Include(p => p.Cita)
                .Where(p => p.ClienteId == clienteId.Value)
                .OrderBy(p => p.Id)
                .AsNoTracking()
                .ToListAsync();

            var modelo = piscinas.Select(p =>
            {
                var proximaVisita = p.Cita
                    .Where(c =>
                        c.FechaHora >= ahora &&
                        c.Estado != "Cancelada" &&
                        c.Estado != "Completada")
                    .OrderBy(c => c.FechaHora)
                    .FirstOrDefault();

                return new PiscinaClienteListViewModel
                {
                    PiscinaId = p.Id,
                    Tipo = p.Tipo,
                    Estado = p.Estado,
                    VolumenM3 = p.VolumenM3,
                    Direccion = ConstruirDireccionCorta(p.Direccion),
                    FotoUrl = "/img/piscina-default.jpg",
                    ProximaVisitaFechaHora = proximaVisita?.FechaHora,
                    ProximaVisitaTipo = proximaVisita?.Tipo,
                    ProximaVisitaEstado = proximaVisita?.Estado
                };
            }).ToList();

            return View(modelo);
        }

        [HttpGet]
        [Authorize(Roles = "3")]
        public async Task<IActionResult> DetalleMiPiscina(int id)
        {
            var clienteId = await ObtenerClienteIdAutenticadoAsync();

            if (clienteId == null)
                return RedirectToAction("InicioSesion", "Auth");

            var ahora = DateTime.Now;

            var piscina = await _contexto.Piscina
                .Include(p => p.Cliente)
                    .ThenInclude(c => c.Usuario)
                .Include(p => p.Direccion)
                    .ThenInclude(d => d.Distrito)
                        .ThenInclude(d => d.Canton)
                            .ThenInclude(c => c.Provincia)
                .Include(p => p.PiscinaEquipamiento)
                    .ThenInclude(pe => pe.Equipamiento)
                .Include(p => p.Cita)
                    .ThenInclude(c => c.Tecnico)
                .Include(p => p.Cita)
                    .ThenInclude(c => c.Servicio)
                        .ThenInclude(s => s!.Inspeccion)
                .Where(p => p.Id == id && p.ClienteId == clienteId.Value)
                .AsSplitQuery()
                .AsNoTracking()
                .FirstOrDefaultAsync();

            if (piscina == null)
                return RedirectToAction("MisPiscinas");

            var proximaVisita = piscina.Cita
                .Where(c =>
                    c.FechaHora >= ahora &&
                    c.Estado != "Cancelada" &&
                    c.Estado != "Completada")
                .OrderBy(c => c.FechaHora)
                .FirstOrDefault();

            var ultimaInspeccion = piscina.Cita
                .Where(c => c.Servicio != null)
                .SelectMany(c => c.Servicio!.Inspeccion)
                .OrderByDescending(i => i.FechaInspeccion)
                .FirstOrDefault();

            var ultimosServicios = piscina.Cita
                .Where(c => c.Servicio != null)
                .OrderByDescending(c => c.FechaHora)
                .Take(3)
                .Select(c => new ServicioPiscinaViewModel
                {
                    ServicioId = c.Servicio!.Id,
                    FechaCita = c.FechaHora,
                    FechaApertura = c.Servicio.FechaApertura,
                    FechaCierre = c.Servicio.FechaCierre,
                    TipoCita = c.Tipo,
                    EstadoServicio = c.Servicio.Estado,
                    TrabajoRealizado = string.IsNullOrWhiteSpace(c.Servicio.TrabajoRealizado)
                        ? "Sin detalle registrado"
                        : c.Servicio.TrabajoRealizado,
                    Tecnico = $"{c.Tecnico.Nombre} {c.Tecnico.ApellidoPaterno}"
                })
                .ToList();

            var modelo = new PiscinaClienteDetalleViewModel
            {
                PiscinaId = piscina.Id,
                Tipo = piscina.Tipo,
                Estado = piscina.Estado,
                VolumenM3 = piscina.VolumenM3,
                DireccionCompleta = ConstruirDireccionCompleta(piscina.Direccion),
                FotoUrl = "/img/piscina-default.jpg",

                ProximaVisitaFechaHora = proximaVisita?.FechaHora,
                ProximaVisitaTipo = proximaVisita?.Tipo,
                ProximaVisitaEstado = proximaVisita?.Estado,
                ProximoTecnico = proximaVisita == null
                    ? null
                    : $"{proximaVisita.Tecnico.Nombre} {proximaVisita.Tecnico.ApellidoPaterno}",

                Equipamientos = piscina.PiscinaEquipamiento
                    .Select(pe => pe.Equipamiento.Nombre)
                    .OrderBy(nombre => nombre)
                    .ToList(),

                UltimosParametros = ultimaInspeccion == null ? null : new ParametrosPiscinaViewModel
                {
                    FechaInspeccion = ultimaInspeccion.FechaInspeccion,
                    CloroPpm = ultimaInspeccion.CloroPpm,
                    Alcalinidad = ultimaInspeccion.Alcalinidad,
                    Ph = ultimaInspeccion.Ph,
                    Calcio = ultimaInspeccion.Calcio,
                    AcidoCianurico = ultimaInspeccion.AcidoCianurico,
                    TemperaturaC = ultimaInspeccion.TemperaturaC,
                    Observaciones = ultimaInspeccion.Observaciones
                },

                UltimosServicios = ultimosServicios
            };

            return View(modelo);
        }

        private async Task<int?> ObtenerUsuarioAutenticadoIdAsync()
        {
            var correo = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(correo))
                return null;

            return await _contexto.Usuario
                .Where(u => u.Correo == correo && u.Activo)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<int?> ObtenerClienteIdAutenticadoAsync()
        {
            var correo = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue(ClaimTypes.Name);

            if (string.IsNullOrWhiteSpace(correo))
                return null;

            return await _contexto.Cliente
                .Where(c => c.Usuario.Correo == correo && c.Usuario.Activo)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync();
        }

        private static void CargarDatosFormularioParametro(RegistrarParametroAguaViewModel modelo, Cita cita)
        {
            modelo.CitaId = cita.Id;
            modelo.PiscinaId = cita.PiscinaId;
            modelo.Cliente = $"{cita.Piscina.Cliente.Usuario.Nombre} {cita.Piscina.Cliente.Usuario.ApellidoPaterno}";
            modelo.Piscina = $"Piscina #{cita.Piscina.Id} - {cita.Piscina.Tipo}";
            modelo.Direccion = ConstruirDireccionCorta(cita.Piscina.Direccion);
            modelo.FechaHoraCita = cita.FechaHora;
            modelo.TipoCita = cita.Tipo;
            modelo.EstadoCita = cita.Estado;
            modelo.MedicionesRegistradas = cita.Servicio?.Inspeccion.Count ?? 0;
        }

        private static string ConstruirDireccionCorta(DireccionCliente? direccion)
        {
            if (direccion == null)
                return "Dirección no registrada";

            var distrito = direccion.Distrito?.Nombre ?? "Distrito no registrado";

            return $"{direccion.Detalles}, {distrito}";
        }

        private static string ConstruirDireccionCompleta(DireccionCliente? direccion)
        {
            if (direccion == null)
                return "Dirección no registrada";

            var provincia = direccion.Distrito?.Canton?.Provincia?.Nombre ?? "Provincia no registrada";
            var canton = direccion.Distrito?.Canton?.Nombre ?? "Cantón no registrado";
            var distrito = direccion.Distrito?.Nombre ?? "Distrito no registrado";

            return $"{direccion.TipoDireccion} — {direccion.Detalles}, {distrito}, {canton}, {provincia}";
        }

        private static readonly List<string> TiposPermitidos = new()
        {
            "Residencial",
            "Comercial",
            "Semiolímpica",
            "Olímpica",
            "Jacuzzi",
            "Otro"
        };

        private static readonly List<string> EstadosPermitidos = new()
        {
            "Activa",
            "Inactiva",
            "En mantenimiento",
            "En construcción"
        };

        private void CargarViewBagEditar(Cliente clienteConNavegacion, int direccionId, string tipo, string estado)
        {
            ViewBag.NombreCliente = $"{clienteConNavegacion.Usuario.Nombre} {clienteConNavegacion.Usuario.ApellidoPaterno}";

            ViewBag.Direcciones = new SelectList(
                clienteConNavegacion.DireccionCliente.Select(d => new
                {
                    d.Id,
                    Texto = $"{d.TipoDireccion} — {d.Detalles}, {d.Distrito.Nombre}"
                }),
                "Id",
                "Texto",
                direccionId
            );

            ViewBag.Tipos = new SelectList(
                TiposPermitidos.Select(t => new { Valor = t, Texto = t }),
                "Valor",
                "Texto",
                tipo
            );

            ViewBag.Estados = new SelectList(
                EstadosPermitidos.Select(e => new { Valor = e, Texto = e }),
                "Valor",
                "Texto",
                estado
            );
        }
    }
}