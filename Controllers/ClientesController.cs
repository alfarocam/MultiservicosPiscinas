using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using MultiserviciosPiscinas.Services;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class ClientesController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;
        private readonly IConfiguration _configuration;

        public ClientesController(PiscinasYMultiserviciosContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<IActionResult> Index()
        {
            var clientes = await _context.Cliente
                .Include(c => c.Usuario)
                .Include(c => c.TelefonosCliente)
                .Include(c => c.DireccionCliente)
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

        private async Task CargarProvinciasAsync()
        {
            ViewBag.Provincias = await _context.Provincia.OrderBy(p => p.Nombre).ToListAsync();
            ViewBag.Cantones = new List<Canton>();
            ViewBag.Distritos = new List<Distrito>();
        }

        private async Task CargarDatosCrearAsync()
        {
            await CargarProvinciasAsync();
            // Mismo patrón que ya usa RutaOptimizadaController para pasarle la
            // API key de Google Maps a la vista (nunca hardcodeada, sale de
            // configuración: appsettings.Development.json / User Secrets en local).
            ViewBag.GoogleMapsApiKey = _configuration["GoogleMaps:ApiKey"] ?? string.Empty;
        }

        private async Task<(bool Existe, string? Campo, int? ClienteId, string? Mensaje, string? UrlPerfil)> BuscarClienteExistenteAsync(string? correo, string? telefono)
        {
            var correoLimpio = correo?.Trim() ?? string.Empty;
            var telefonoNormalizado = Generales.NormalizarTelefono(telefono);

            var clientePorCorreo = string.IsNullOrEmpty(correoLimpio)
                ? null
                : await _context.Cliente
                    .Where(c => c.Usuario.Correo == correoLimpio)
                    .Select(c => new
                    {
                        c.Id,
                        NombreCompleto = c.Usuario.Nombre + " " + c.Usuario.ApellidoPaterno
                    })
                    .FirstOrDefaultAsync();

            var clientePorTelefono = string.IsNullOrEmpty(telefonoNormalizado)
                ? null
                : await _context.TelefonosCliente
                    .Where(t => t.NumeroTelefono.Trim().Replace("-", "").Replace(" ", "") == telefonoNormalizado)
                    .Select(t => new
                    {
                        Id = t.ClienteId,
                        NombreCompleto = t.Cliente.Usuario.Nombre + " " + t.Cliente.Usuario.ApellidoPaterno
                    })
                    .FirstOrDefaultAsync();

            if (clientePorTelefono != null && clientePorCorreo != null)
            {
                var url = Url.Action(nameof(Detalle), "Clientes", new { id = clientePorTelefono.Id });
                var mensaje = clientePorTelefono.Id == clientePorCorreo.Id
                    ? $"Ya existe un cliente registrado con ese correo y teléfono ({clientePorTelefono.NombreCompleto})."
                    : $"Ya existen clientes registrados con ese correo ({clientePorCorreo.NombreCompleto}) y con ese teléfono ({clientePorTelefono.NombreCompleto}).";
                return (true, "ambos", clientePorTelefono.Id, mensaje, url);
            }

            if (clientePorTelefono != null)
            {
                var url = Url.Action(nameof(Detalle), "Clientes", new { id = clientePorTelefono.Id });
                return (true, "telefono", clientePorTelefono.Id,
                    $"Ya existe un cliente registrado con ese teléfono ({clientePorTelefono.NombreCompleto}).", url);
            }

            if (clientePorCorreo != null)
            {
                var url = Url.Action(nameof(Detalle), "Clientes", new { id = clientePorCorreo.Id });
                return (true, "correo", clientePorCorreo.Id,
                    $"Ya existe un cliente registrado con ese correo ({clientePorCorreo.NombreCompleto}).", url);
            }

            bool usuarioExiste = !string.IsNullOrEmpty(correoLimpio) &&
                await _context.Usuario.AnyAsync(u => u.Correo == correoLimpio);

            if (usuarioExiste)
            {
                return (true, "correo", null, "Ya existe un usuario registrado con ese correo.", null);
            }

            return (false, null, null, null, null);
        }

        [HttpGet]
        public async Task<IActionResult> VerificarDuplicado(string? correo, string? telefono)
        {
            var resultado = await BuscarClienteExistenteAsync(correo, telefono);
            return Json(new
            {
                existe = resultado.Existe,
                campo = resultado.Campo,
                clienteId = resultado.ClienteId,
                mensaje = resultado.Mensaje,
                urlPerfil = resultado.UrlPerfil
            });
        }

        public async Task<IActionResult> Crear()
        {
            await CargarDatosCrearAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ClienteCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await CargarDatosCrearAsync();
                return View(model);
            }

            model.Correo = model.Correo.Trim();
            model.Telefono = model.Telefono.Trim();

            var duplicado = await BuscarClienteExistenteAsync(model.Correo, model.Telefono);
            if (duplicado.Existe)
            {
                if (duplicado.Campo is "telefono" or "ambos")
                {
                    ModelState.AddModelError(nameof(model.Telefono), duplicado.Mensaje ?? "Ya existe un cliente con ese teléfono.");
                }

                if (duplicado.Campo is "correo" or "ambos")
                {
                    ModelState.AddModelError(nameof(model.Correo), duplicado.Mensaje ?? "Ya existe un usuario con ese correo.");
                }

                ViewBag.ClienteExistenteMensaje = duplicado.Mensaje;
                ViewBag.ClienteExistenteUrl = duplicado.UrlPerfil;
                await CargarDatosCrearAsync();
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

            return View(cliente);
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