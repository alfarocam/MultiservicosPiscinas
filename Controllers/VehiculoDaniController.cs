using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MultiserviciosPiscinas.Models;

namespace MultiserviciosPiscinas.Controllers
{
    public class VehiculoDaniController : Controller
    {
        private readonly PiscinasYMultiserviciosContext _context;

        public VehiculoDaniController(PiscinasYMultiserviciosContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var vehiculos = await _context.Vehiculo
                .Include(v => v.Tecnico)
                .OrderByDescending(v => v.Id)
                .ToListAsync();

            var tecnicos = await _context.Usuario
                .Select(u => new {
                    u.Id,
                    NombreCompleto = u.Nombre + " " + u.ApellidoPaterno + " " + u.ApellidoMaterno
                })
                .ToListAsync();

            ViewBag.Tecnicos = new SelectList(tecnicos, "Id", "NombreCompleto");

            return View(vehiculos);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Vehiculo vehiculo)
        {
            bool placaExiste = await _context.Vehiculo
                .AnyAsync(v => v.Placa.ToLower() == vehiculo.Placa.ToLower());

            if (placaExiste)
            {
                TempData["MensajeError"] = $"¡Error! La placa '{vehiculo.Placa}' ya está registrada en el sistema.";
                return RedirectToAction(nameof(Index));
            }

            // registro exitoso
            vehiculo.Estado = "Activo"; 

            _context.Vehiculo.Add(vehiculo);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = $"Vehiiculo registrado con exito (ID Interno: #{vehiculo.Id}) y asignado como Activo.";
            return RedirectToAction(nameof(Index));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Vehiculo vehiculo)
        {
            var vehiculoExistente = await _context.Vehiculo.FindAsync(vehiculo.Id);

            if (vehiculoExistente == null)
            {
                TempData["MensajeError"] = "No se encontró el vehiculo para actualizar.";
                return RedirectToAction(nameof(Index));
            }

            
            bool placaDuplicada = await _context.Vehiculo
                .AnyAsync(v => v.Placa.ToLower() == vehiculo.Placa.ToLower() && v.Id != vehiculo.Id);

            if (placaDuplicada)
            {
                TempData["MensajeError"] = $"¡Error! La placa '{vehiculo.Placa}' pertenece a otro vehículo.";
                return RedirectToAction(nameof(Index));
            }

            
            vehiculoExistente.Placa = vehiculo.Placa;
            vehiculoExistente.Marca = vehiculo.Marca;
            vehiculoExistente.Modelo = vehiculo.Modelo;
            vehiculoExistente.TecnicoId = vehiculo.TecnicoId;

            _context.Update(vehiculoExistente);
            await _context.SaveChangesAsync();

            
            var usuarioActual = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "Admin";
            var fechaModificacion = DateTime.Now.ToString("dd/MM/yyyy hh:mm tt");

            TempData["MensajeExito"] = $"Vehículo #{vehiculo.Id} actualizado correctamente. (Última modificación por: {usuarioActual} el {fechaModificacion})";
            return RedirectToAction(nameof(Index));
        }
    }
}