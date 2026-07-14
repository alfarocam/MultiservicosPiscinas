using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;
using System.Security.Claims;

namespace MultiserviciosPiscinas.Controllers
{
    [Authorize(Roles = "1,2")]
    public class TablonAnunciosController(PiscinasYMultiserviciosContext context) : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context = context;

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Today);

            var anuncios = await _context.Anuncio
                .Include(a => a.Autor)
                .Where(a => a.FechaPublicacion <= hoy
                         && (a.FechaCaducidad == null || a.FechaCaducidad >= hoy))
                .OrderByDescending(a => a.FechaPublicacion)
                .ThenByDescending(a => a.Id)
                .AsNoTracking()
                .ToListAsync();

            return View(anuncios);
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(AnuncioCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var correo = User.FindFirst(ClaimTypes.Email)?.Value;
            var autor = await _context.Usuario.FirstOrDefaultAsync(u => u.Correo == correo);

            if (autor == null)
            {
                return Unauthorized();
            }

            var anuncio = new Anuncio
            {
                AutorId = autor.Id,
                Titulo = model.Titulo.Trim(),
                Contenido = model.Contenido.Trim(),
                Prioridad = model.Urgente ? "Urgente" : "Normal",
                FechaPublicacion = DateOnly.FromDateTime(DateTime.Now),
                FechaCaducidad = null
            };

            _context.Anuncio.Add(anuncio);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Comunicado publicado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "1")]
        [HttpGet]
        public async Task<IActionResult> Editar(int id)
        {
            var anuncio = await _context.Anuncio.FindAsync(id);

            if (anuncio == null)
            {
                return NotFound();
            }

            var model = new AnuncioEditarViewModel
            {
                Id = anuncio.Id,
                Titulo = anuncio.Titulo,
                Contenido = anuncio.Contenido,
                Urgente = anuncio.Prioridad == "Urgente"
            };

            return View(model);
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, AnuncioEditarViewModel model)
        {
            if (id != model.Id)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var anuncio = await _context.Anuncio.FindAsync(id);

            if (anuncio == null)
            {
                return NotFound();
            }

            anuncio.Titulo = model.Titulo.Trim();
            anuncio.Contenido = model.Contenido.Trim();
            anuncio.Prioridad = model.Urgente ? "Urgente" : "Normal";

            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Comunicado actualizado correctamente.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "1")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var anuncio = await _context.Anuncio.FindAsync(id);

            if (anuncio == null)
            {
                return NotFound();
            }

            _context.Anuncio.Remove(anuncio);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Comunicado eliminado correctamente.";
            return RedirectToAction(nameof(Index));
        }
    }
}