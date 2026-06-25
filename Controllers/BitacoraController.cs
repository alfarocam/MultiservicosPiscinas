using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1")]
    public class BitacoraController(PiscinasYMultiserviciosContext context) : Controller
    {

        public async Task<IActionResult> Index(string accionFiltro, string tablaFiltro, string ordenFecha)
        {
            // mostrar si existen registros
            var consulta = context.BitacoraAuditoria.Include(b => b.Usuario).AsQueryable();

            // filtro de registros

            if (!string.IsNullOrEmpty(accionFiltro))
            {
                consulta = consulta.Where(b => b.Accion == accionFiltro);
            }

            if (!string.IsNullOrEmpty(tablaFiltro))
            {
                consulta = consulta.Where(b => b.TablaAfectada.Contains(tablaFiltro));
            }

            // ordenar registros

            if (ordenFecha == "asc")
            {
                consulta = consulta.OrderBy(b => b.FechaHora);
            }
            else
            {
                consulta = consulta.OrderByDescending(b => b.FechaHora);
            }


            ViewBag.AccionActual = accionFiltro;
            ViewBag.TablaActual = tablaFiltro;
            ViewBag.OrdenActual = ordenFecha;


            ViewBag.AccionesDisponibles = new List<string> { "INSERT", "UPDATE", "DELETE" };

            var resultado = await consulta.ToListAsync();
            return View(resultado);
        }

        public IActionResult Registrar() => RedirectToAction(nameof(Index));
        public IActionResult Detalle(int id) => RedirectToAction(nameof(Index));
    }
}