using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    public class GastosController : Controller
    {

        private readonly PiscinasYMultiserviciosContext _context;
        private readonly IWebHostEnvironment _env;

        public GastosController(PiscinasYMultiserviciosContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> Registrar()
        {
            //carga categorias
            ViewBag.Categorias = new SelectList(await _context.CategoriaGastoOperativo.ToListAsync(), "Id", "NombreCategoria");

            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrar(GastoOperativo gasto, IFormFile? comprobanteArchivo)
        {
            try
            {

                gasto.Estado = "Pendiente";
                gasto.Fecha = DateOnly.FromDateTime(DateTime.Now);


                // uso usuario 1 
                gasto.UsuarioId = 1;


                if (comprobanteArchivo != null && comprobanteArchivo.Length > 0)
                {
                    string carpetaSubidas = Path.Combine(_env.WebRootPath, "comprobantes");
                    if (!Directory.Exists(carpetaSubidas))
                    {
                        Directory.CreateDirectory(carpetaSubidas);
                    }

                    string nombreArchivoUnique = Guid.NewGuid().ToString() + Path.GetExtension(comprobanteArchivo.FileName);
                    string rutaFisica = Path.Combine(carpetaSubidas, nombreArchivoUnique);

                    using (var stream = new FileStream(rutaFisica, FileMode.Create))
                    {
                        await comprobanteArchivo.CopyToAsync(stream);
                    }

                    gasto.ComprobanteRuta = "/comprobantes/" + nombreArchivoUnique;
                }


                _context.Add(gasto);
                await _context.SaveChangesAsync();

                TempData["MensajeExito"] = "El gasto se registro correctamente y quedo en estado Pendiente.";
                return RedirectToAction(nameof(Registrar));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Ocurrió un error al guardar: " + ex.Message);
            }

            ViewBag.Categorias = new SelectList(await _context.CategoriaGastoOperativo.ToListAsync(), "Id", "NombreCategoria", gasto.CategoriaId);
            return View(gasto);
        }
    }
}