using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Data;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize]
    public class PiscinasController(PiscinasYMultiserviciosContext _contexto) : Controller
    {

        [HttpGet]
        public IActionResult Index()
        {
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
        public IActionResult Detalle()
        {
            return View();
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
                piscinaExistente.Tipo        = piscina.Tipo;
                piscinaExistente.VolumenM3   = piscina.VolumenM3;
                piscinaExistente.Estado      = piscina.Estado;

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

        private static readonly List<string> TiposPermitidos  = ["Residencial", "Comercial", "Semiolímpica", "Olímpica", "Jacuzzi", "Otro"];
        private static readonly List<string> EstadosPermitidos = ["Activa", "Inactiva", "En mantenimiento", "En construcción"];

        private void CargarViewBagEditar(Cliente clienteConNavegacion, int direccionId, string tipo, string estado)
        {
            ViewBag.NombreCliente = $"{clienteConNavegacion.Usuario.Nombre} {clienteConNavegacion.Usuario.ApellidoPaterno}";

            ViewBag.Direcciones = new SelectList(
                clienteConNavegacion.DireccionCliente.Select(d => new
                {
                    d.Id,
                    Texto = $"{d.TipoDireccion} — {d.Detalles}, {d.Distrito.Nombre}"
                }),
                "Id", "Texto", direccionId
            );

            ViewBag.Tipos   = new SelectList(TiposPermitidos.Select(t  => new { Valor = t, Texto = t  }), "Valor", "Texto", tipo);
            ViewBag.Estados = new SelectList(EstadosPermitidos.Select(e => new { Valor = e, Texto = e }), "Valor", "Texto", estado);
        }

        

    }
}
