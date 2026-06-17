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
            return View();
        }

        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Piscina piscina)
        {
            if (ModelState.IsValid)
            {
                _contexto.Piscina.Add(piscina);
                _contexto.SaveChanges();
                return RedirectToAction("Index");
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
