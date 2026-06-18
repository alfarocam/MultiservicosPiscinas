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
        public IActionResult Detalle()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Editar()
        {
            return View();
        }

        

    }
}
