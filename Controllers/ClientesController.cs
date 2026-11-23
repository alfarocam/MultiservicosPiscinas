using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Data;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;
using System.Text.RegularExpressions;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class ClientesController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;
        private readonly IConfiguration _configuration;
        private readonly BitacoraService _bitacora;

        public ClientesController(
            PiscinasYMultiserviciosContext context,
            IConfiguration configuration,
            BitacoraService bitacora)
        {
            _context = context;
            _configuration = configuration;
            _bitacora = bitacora;
        }

        public async Task<IActionResult> Index()
        {
            await DivisionTerritorialSeeder.AsegurarIntegridadClientesYTelefonosAsync(_context);

            var clientes = await _context.Cliente
                .Include(c => c.Usuario)
                .Include(c => c.TelefonosCliente)
                .Include(c => c.DireccionCliente)
                .OrderBy(c => c.Usuario.Nombre)
                .ThenBy(c => c.Usuario.ApellidoPaterno)
                .ToListAsync();

            return View(clientes);
        }

        public async Task<IActionResult> Detalle(int id)
        {
            var cliente = await _context.Cliente
                .Include(c => c.Usuario)
                .Include(c => c.TelefonosCliente)
                .Include(c => c.DireccionCliente)
                .Include(c => c.Piscina)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            return View(cliente);
        }

        private async Task CargarDatosUbicacionClienteAsync(int? distritoId = null)
        {
            ViewBag.Provincias = await _context.Provincia.OrderBy(p => p.Nombre).ToListAsync();
            ViewBag.Cantones = new List<Canton>();
            ViewBag.Distritos = new List<Distrito>();
            ViewBag.SelectedProvinciaId = 0;
            ViewBag.SelectedCantonId = 0;
            ViewBag.SelectedDistritoId = 0;

            if (distritoId.HasValue && distritoId.Value > 0)
            {
                var distrito = await _context.Distrito
                    .Include(d => d.Canton)
                    .FirstOrDefaultAsync(d => d.Id == distritoId.Value);

                if (distrito?.Canton != null)
                {
                    ViewBag.SelectedProvinciaId = distrito.Canton.ProvinciaId;
                    ViewBag.SelectedCantonId = distrito.CantonId;
                    ViewBag.SelectedDistritoId = distrito.Id;

                    ViewBag.Cantones = await _context.Canton
                        .Where(c => c.ProvinciaId == distrito.Canton.ProvinciaId)
                        .OrderBy(c => c.Nombre)
                        .ToListAsync();

                    ViewBag.Distritos = await _context.Distrito
                        .Where(d => d.CantonId == distrito.CantonId)
                        .OrderBy(d => d.Nombre)
                        .ToListAsync();
                }
            }

            // Mismo patrón que ya usa RutaOptimizadaController para pasarle la
            // API key de Google Maps a la vista (nunca hardcodeada, sale de
            // configuración: appsettings.Development.json / User Secrets en local).
            ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"] ?? string.Empty;
        }

        private static bool EsCorreoValido(string correo) =>
            Regex.IsMatch(
                correo,
                @"^[a-zA-Z0-9._%+-]+@(gmail\.com|hotmail\.com|yahoo\.com|outlook\.com|icloud\.com|live\.com|piscinas\.com|multiservicios\.com|test\.com|correo\.com|ejemplo\.com)$",
                RegexOptions.IgnoreCase);

        private async Task<(bool Existe, string? Campo, bool DuplicadoTelefono, bool DuplicadoCorreo, int? ClienteId, string? Mensaje, string? UrlPerfil)> BuscarClienteExistenteAsync(
            string? correo,
            string? telefono,
            int? excluirClienteId = null)
        {
            var correoLimpio = correo?.Trim() ?? string.Empty;
            var telefonoNormalizado = Generales.NormalizarTelefono(telefono);

            var clientePorCorreo = string.IsNullOrEmpty(correoLimpio)
                ? null
                : await _context.Cliente
                    .Where(c => (!excluirClienteId.HasValue || c.Id != excluirClienteId.Value) &&
                                c.Usuario.Correo == correoLimpio)
                    .Select(c => new
                    {
                        c.Id,
                        NombreCompleto = c.Usuario.Nombre + " " + c.Usuario.ApellidoPaterno
                    })
                    .FirstOrDefaultAsync();

            var clientePorTelefono = string.IsNullOrEmpty(telefonoNormalizado)
                ? null
                : await _context.TelefonosCliente
                    .Where(t => (!excluirClienteId.HasValue || t.ClienteId != excluirClienteId.Value) &&
                                t.NumeroTelefono.Trim().Replace("-", "").Replace(" ", "") == telefonoNormalizado)
                    .Select(t => new
                    {
                        Id = t.ClienteId,
                        NombreCompleto = t.Cliente.Usuario.Nombre + " " + t.Cliente.Usuario.ApellidoPaterno
                    })
                    .FirstOrDefaultAsync();

            int? usuarioExcluidoId = excluirClienteId.HasValue
                ? await _context.Cliente
                    .Where(c => c.Id == excluirClienteId.Value)
                    .Select(c => (int?)c.UsuarioId)
                    .FirstOrDefaultAsync()
                : null;

            bool usuarioExiste = clientePorCorreo == null &&
                !string.IsNullOrEmpty(correoLimpio) &&
                await _context.Usuario.AnyAsync(u =>
                    u.Correo == correoLimpio &&
                    (!usuarioExcluidoId.HasValue || u.Id != usuarioExcluidoId.Value));

            bool dupTelefono = clientePorTelefono != null;
            bool dupCorreo = clientePorCorreo != null || usuarioExiste;

            if (!dupTelefono && !dupCorreo)
            {
                return (false, null, false, false, null, null, null);
            }

            if (usuarioExiste && !dupTelefono && clientePorCorreo == null)
            {
                return (true, "correo", false, true, null,
                    "Ya existe un usuario registrado con ese correo.", null);
            }

            if (clientePorTelefono != null && clientePorCorreo != null)
            {
                var urlAmbos = Url.Action(nameof(Detalle), "Clientes", new { id = clientePorTelefono.Id });
                var mensajeAmbos = clientePorTelefono.Id == clientePorCorreo.Id
                    ? $"Ya existe un cliente registrado con ese correo y teléfono ({clientePorTelefono.NombreCompleto})."
                    : $"Ya existen clientes registrados con ese correo ({clientePorCorreo.NombreCompleto}) y con ese teléfono ({clientePorTelefono.NombreCompleto}).";
                return (true, "ambos", true, true, clientePorTelefono.Id, mensajeAmbos, urlAmbos);
            }

            if (clientePorTelefono != null)
            {
                var urlTel = Url.Action(nameof(Detalle), "Clientes", new { id = clientePorTelefono.Id });
                return (true, "telefono", true, false, clientePorTelefono.Id,
                    $"Ya existe un cliente registrado con ese teléfono ({clientePorTelefono.NombreCompleto}).", urlTel);
            }

            var urlCorreo = Url.Action(nameof(Detalle), "Clientes", new { id = clientePorCorreo!.Id });
            return (true, "correo", false, true, clientePorCorreo.Id,
                $"Ya existe un cliente registrado con ese correo ({clientePorCorreo.NombreCompleto}).", urlCorreo);
        }

        [HttpGet]
        public async Task<IActionResult> VerificarDuplicado(
            string? correo,
            string? telefono,
            int? excluirClienteId = null)
        {
            var resultado = await BuscarClienteExistenteAsync(
                correo,
                telefono,
                excluirClienteId);

            return Json(new
            {
                existe = resultado.Existe,
                campo = resultado.Campo,
                duplicadoTelefono = resultado.DuplicadoTelefono,
                duplicadoCorreo = resultado.DuplicadoCorreo,
                clienteId = resultado.ClienteId,
                mensaje = resultado.Mensaje,
                urlPerfil = resultado.UrlPerfil
            });
        }

        public async Task<IActionResult> Crear()
        {
            await CargarDatosUbicacionClienteAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ClienteCreateViewModel model)
        {
            model.Nombre = (model.Nombre ?? string.Empty).Trim();
            model.ApellidoPaterno = (model.ApellidoPaterno ?? string.Empty).Trim();
            model.ApellidoMaterno = (model.ApellidoMaterno ?? string.Empty).Trim();
            model.Correo = (model.Correo ?? string.Empty).Trim();
            model.Telefono = (model.Telefono ?? string.Empty).Trim();
            model.Direccion = (model.Direccion ?? string.Empty).Trim();

            if (!string.IsNullOrWhiteSpace(model.Correo) && !EsCorreoValido(model.Correo))
            {
                ModelState.AddModelError(nameof(model.Correo),
                    "Ingresá un correo válido (@gmail.com, @hotmail.com, @yahoo.com, @outlook.com o institucional).");
            }

            if (!string.IsNullOrWhiteSpace(model.Telefono) && !Generales.EsTelefonoValido(model.Telefono))
            {
                ModelState.AddModelError(nameof(model.Telefono),
                    "El teléfono debe contener 8 dígitos con el formato 8888-8888.");
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosUbicacionClienteAsync(model.DistritoId);
                return View(model);
            }

            model.Telefono = Generales.FormatearTelefono(model.Telefono);

            var duplicado = await BuscarClienteExistenteAsync(
                model.Correo,
                model.Telefono);

            if (duplicado.Existe)
            {
                if (duplicado.DuplicadoTelefono)
                {
                    ModelState.AddModelError(nameof(model.Telefono), duplicado.Mensaje ?? "Ya existe un cliente con ese teléfono.");
                }

                if (duplicado.DuplicadoCorreo)
                {
                    ModelState.AddModelError(nameof(model.Correo), duplicado.Mensaje ?? "Ya existe un usuario con ese correo.");
                }

                ViewBag.ClienteExistenteMensaje = duplicado.Mensaje;
                ViewBag.ClienteExistenteUrl = duplicado.UrlPerfil;
                await CargarDatosUbicacionClienteAsync(model.DistritoId);
                return View(model);
            }

            var usuario = new Usuario
            {
                Nombre = model.Nombre,
                ApellidoPaterno = model.ApellidoPaterno,
                ApellidoMaterno = model.ApellidoMaterno,
                Correo = model.Correo,
                Contrasena = "Temporal123",
                RolId = 3,
                Activo = true,
                FechaCreacion = DateTime.Now
            };

            _context.Usuario.Add(usuario);
            await _context.SaveChangesAsync();

            var cliente = new Cliente
            {
                UsuarioId = usuario.Id,
                Notas = ""
            };

            _context.Cliente.Add(cliente);
            await _context.SaveChangesAsync();

            var telefono = new TelefonosCliente
            {
                ClienteId = cliente.Id,
                TipoTelefono = "Principal",
                NumeroTelefono = model.Telefono,
                EsPrincipal = 1
            };

            _context.TelefonosCliente.Add(telefono);

            var direccion = new DireccionCliente
            {
                ClienteId = cliente.Id,
                DistritoId = model.DistritoId,
                TipoDireccion = "Principal",
                Detalles = model.Direccion,
                EsPrincipal = 1,
                Latitud = model.Latitud,
                Longitud = model.Longitud
            };

            _context.DireccionCliente.Add(direccion);

            await _context.SaveChangesAsync();

            await _bitacora.RegistrarAsync(
                User,
                "INSERT",
                "cli.CLIENTE",
                cliente.Id,
                $"Nombre: {usuario.Nombre} {usuario.ApellidoPaterno} {usuario.ApellidoMaterno} | Correo: {usuario.Correo} | Teléfono: {model.Telefono}");

            TempData["Exito"] = "Cliente registrado correctamente.";

            return RedirectToAction(nameof(Detalle), new { id = cliente.Id });
        }

        public async Task<IActionResult> Editar(int id)
        {
            var cliente = await _context.Cliente
                .Include(c => c.Usuario)
                .Include(c => c.TelefonosCliente)
                .Include(c => c.DireccionCliente)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            var telefonoPrincipal = cliente.TelefonosCliente
                .OrderByDescending(t => t.EsPrincipal)
                .ThenBy(t => t.Id)
                .FirstOrDefault();

            var direccionPrincipal = cliente.DireccionCliente
                .OrderByDescending(d => d.EsPrincipal)
                .ThenBy(d => d.Id)
                .FirstOrDefault();

            var vm = new ClienteEditViewModel
            {
                Id = cliente.Id,
                UsuarioId = cliente.UsuarioId,
                Nombre = cliente.Usuario.Nombre,
                ApellidoPaterno = cliente.Usuario.ApellidoPaterno,
                ApellidoMaterno = cliente.Usuario.ApellidoMaterno,
                Correo = cliente.Usuario.Correo,
                Telefono = Generales.FormatearTelefono(telefonoPrincipal?.NumeroTelefono),
                DistritoId = direccionPrincipal?.DistritoId ?? 0,
                Direccion = direccionPrincipal?.Detalles ?? string.Empty,
                Latitud = direccionPrincipal?.Latitud,
                Longitud = direccionPrincipal?.Longitud,
                Notas = cliente.Notas
            };

            await CargarDatosUbicacionClienteAsync(vm.DistritoId);
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ClienteEditViewModel model)
        {
            model.Nombre = (model.Nombre ?? string.Empty).Trim();
            model.ApellidoPaterno = (model.ApellidoPaterno ?? string.Empty).Trim();
            model.ApellidoMaterno = (model.ApellidoMaterno ?? string.Empty).Trim();
            model.Correo = (model.Correo ?? string.Empty).Trim();
            model.Telefono = (model.Telefono ?? string.Empty).Trim();
            model.Direccion = (model.Direccion ?? string.Empty).Trim();
            model.Notas = model.Notas?.Trim();

            if (!string.IsNullOrWhiteSpace(model.Correo) && !EsCorreoValido(model.Correo))
            {
                ModelState.AddModelError(nameof(model.Correo),
                    "Ingresá un correo válido (@gmail.com, @hotmail.com, @yahoo.com, @outlook.com o institucional).");
            }

            if (!string.IsNullOrWhiteSpace(model.Telefono) && !Generales.EsTelefonoValido(model.Telefono))
            {
                ModelState.AddModelError(nameof(model.Telefono),
                    "El teléfono debe contener 8 dígitos con el formato 8888-8888.");
            }

            if (!ModelState.IsValid)
            {
                await CargarDatosUbicacionClienteAsync(model.DistritoId);
                return View(model);
            }

            model.Telefono = Generales.FormatearTelefono(model.Telefono);

            var duplicado = await BuscarClienteExistenteAsync(
                model.Correo,
                model.Telefono,
                excluirClienteId: model.Id);

            if (duplicado.Existe)
            {
                if (duplicado.DuplicadoTelefono)
                {
                    ModelState.AddModelError(nameof(model.Telefono), duplicado.Mensaje ?? "Ya existe un cliente con ese teléfono.");
                }

                if (duplicado.DuplicadoCorreo)
                {
                    ModelState.AddModelError(nameof(model.Correo), duplicado.Mensaje ?? "Ya existe un usuario con ese correo.");
                }

                ViewBag.ClienteExistenteMensaje = duplicado.Mensaje;
                ViewBag.ClienteExistenteUrl = duplicado.UrlPerfil;
                await CargarDatosUbicacionClienteAsync(model.DistritoId);
                return View(model);
            }

            var cliente = await _context.Cliente
                .Include(c => c.Usuario)
                .Include(c => c.TelefonosCliente)
                .Include(c => c.DireccionCliente)
                .FirstOrDefaultAsync(c => c.Id == model.Id);

            if (cliente == null)
                return NotFound();

            var telefonoPrincipal = cliente.TelefonosCliente
                .OrderByDescending(t => t.EsPrincipal)
                .ThenBy(t => t.Id)
                .FirstOrDefault();

            var direccionPrincipal = cliente.DireccionCliente
                .OrderByDescending(d => d.EsPrincipal)
                .ThenBy(d => d.Id)
                .FirstOrDefault();

            var valorAnterior = $"Nombre: {cliente.Usuario.Nombre} {cliente.Usuario.ApellidoPaterno} {cliente.Usuario.ApellidoMaterno} | Correo: {cliente.Usuario.Correo} | Teléfono: {telefonoPrincipal?.NumeroTelefono ?? "N/A"} | Dirección: {direccionPrincipal?.Detalles ?? "N/A"} | Notas: {cliente.Notas ?? ""}";

            cliente.Usuario.Nombre = model.Nombre;
            cliente.Usuario.ApellidoPaterno = model.ApellidoPaterno;
            cliente.Usuario.ApellidoMaterno = model.ApellidoMaterno;
            cliente.Usuario.Correo = model.Correo;
            cliente.Notas = model.Notas ?? string.Empty;

            if (telefonoPrincipal != null)
            {
                telefonoPrincipal.NumeroTelefono = model.Telefono;
                telefonoPrincipal.EsPrincipal = 1;
            }
            else
            {
                _context.TelefonosCliente.Add(new TelefonosCliente
                {
                    ClienteId = cliente.Id,
                    TipoTelefono = "Principal",
                    NumeroTelefono = model.Telefono,
                    EsPrincipal = 1
                });
            }

            if (direccionPrincipal != null)
            {
                direccionPrincipal.DistritoId = model.DistritoId;
                direccionPrincipal.Detalles = model.Direccion;
                direccionPrincipal.Latitud = model.Latitud;
                direccionPrincipal.Longitud = model.Longitud;
                direccionPrincipal.EsPrincipal = 1;
            }
            else
            {
                _context.DireccionCliente.Add(new DireccionCliente
                {
                    ClienteId = cliente.Id,
                    DistritoId = model.DistritoId,
                    TipoDireccion = "Principal",
                    Detalles = model.Direccion,
                    EsPrincipal = 1,
                    Latitud = model.Latitud,
                    Longitud = model.Longitud
                });
            }

            await _context.SaveChangesAsync();

            var valorNuevo = $"Nombre: {cliente.Usuario.Nombre} {cliente.Usuario.ApellidoPaterno} {cliente.Usuario.ApellidoMaterno} | Correo: {cliente.Usuario.Correo} | Teléfono: {model.Telefono} | Dirección: {model.Direccion} | Notas: {cliente.Notas}";
            await _bitacora.RegistrarAsync(
                User,
                "UPDATE",
                "cli.CLIENTE",
                cliente.Id,
                valorNuevo,
                valorAnterior);

            TempData["Exito"] = "Datos del cliente actualizados correctamente.";
            return RedirectToAction(nameof(Detalle), new { id = cliente.Id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var cliente = await _context.Cliente
                .Include(c => c.Usuario)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound();

            cliente.Usuario.Activo = false;

            await _context.SaveChangesAsync();

            TempData["Exito"] = "Cliente desactivado correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}